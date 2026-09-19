"""Testes HTTP com PostgreSQL isolado: compose.integration.yaml, porta 18080."""
import concurrent.futures
import json
import random
import urllib.error
import urllib.request
import uuid

BASE = "http://127.0.0.1:18080"
count = 0


def request(method, path, body=None, expected=200):
    global count
    data = None if body is None else json.dumps(body).encode()
    req = urllib.request.Request(BASE + path, data=data, method=method,
                                 headers={"Content-Type": "application/json"})
    try:
        response = urllib.request.urlopen(req, timeout=20)
    except urllib.error.HTTPError as error:
        response = error
    with response:
        raw = response.read()
        payload = json.loads(raw) if raw else None
        allowed = expected if isinstance(expected, tuple) else (expected,)
        assert response.status in allowed, (method, path, response.status, payload)
        if response.status >= 400:
            assert response.headers["Content-Type"].startswith("application/problem+json")
        count += 1
        return payload, response.status


def api(method, path, body=None, expected=200):
    return request(method, path, body, expected)[0]


def run():
    product_id = random.randint(10000, 1000000000)
    coupon_id = product_id
    p = f"/api/produtos/{product_id}"
    q = f"/api/cupons/{coupon_id}"
    product = {"id": product_id, "descricaoProduto": "Produto teste", "precoLiquido": 19.99, "quantidadeEstoque": 10}
    api("GET", "/health/ready")
    schema = api("GET", "/openapi/v1.json")
    assert "post" in schema["paths"]["/api/carrinhos/{id}/checkout"]
    api("POST", "/api/produtos", product, 201)
    api("POST", "/api/produtos", product, 409)
    assert api("GET", p)["precoLiquido"] == 19.99
    assert any(x["id"] == product_id for x in api("GET", "/api/produtos"))
    api("PUT", p, {**product, "precoLiquido": -1}, 422)
    api("PUT", p, {**product, "precoLiquido": 1.001}, 422)
    api("PUT", p, {**product, "quantidadeEstoque": -1}, 422)
    api("PUT", p, product)
    coupon = {"id": coupon_id, "codigoCupom": f"TEST{coupon_id}", "percentualDesconto": 5}
    api("POST", "/api/cupons", coupon, 201)
    api("POST", "/api/cupons", {**coupon, "id": coupon_id + 1}, 409)
    api("PUT", q, {**coupon, "percentualDesconto": 101}, 422)
    api("PUT", q, {**coupon, "percentualDesconto": 20})
    assert api("GET", q)["percentualDesconto"] == 20
    assert {"10OFF", "15OFF"} <= {x["codigoCupom"] for x in api("GET", "/api/cupons")}

    cart = api("POST", "/api/carrinhos", expected=201)
    c = "/api/carrinhos/" + cart["id"]
    api("POST", c + "/checkout", expected=422)
    item = {"produtoId": product_id, "quantidade": 5}
    result = api("POST", c + "/itens", item)
    assert result["itens"][0]["quantidade"] == 1
    result = api("POST", c + "/itens", {**item, "quantidade": 2})
    assert result["subtotal"] == 59.97
    api("POST", c + "/itens", {**item, "quantidade": 8}, 422)
    api("PUT", c + f"/itens/{product_id}", {"quantidade": 0}, 400)
    api("PUT", c + f"/itens/{product_id}", {"quantidade": 11}, 422)
    result = api("PUT", c + "/cupom", {"codigoCupom": "10off"})
    assert result["desconto"] == 6 and result["total"] == 53.97
    result = api("PUT", c + "/cupom", {"codigoCupom": "15OFF"})
    assert result["desconto"] == 9 and result["cupom"]["codigoCupom"] == "15OFF"
    api("PUT", c + "/cupom", {"codigoCupom": "INVALIDO"}, 404)
    result = api("PUT", c + f"/itens/{product_id}", {"quantidade": 2})
    assert result["subtotal"] == 39.98 and result["total"] == 33.98
    result = api("DELETE", c + "/cupom")
    assert result["desconto"] == 0 and result["total"] == 39.98
    api("DELETE", p, expected=409)
    api("PUT", c + "/cupom", {"codigoCupom": coupon["codigoCupom"]})
    api("DELETE", q, expected=409)
    api("DELETE", c + "/cupom")
    api("DELETE", q, expected=204)
    api("GET", q, expected=404)
    result = api("DELETE", c + f"/itens/{product_id}")
    assert result["subtotal"] == 0 and result["itens"] == []
    api("DELETE", c + f"/itens/{product_id}", expected=404)
    api("DELETE", c, expected=204)
    api("GET", c, expected=404)
    api("DELETE", p, expected=204)
    api("GET", p, expected=404)

    # Checkout concorrente: apenas uma compra pode consumir a última unidade.
    api("POST", "/api/produtos", {**product, "quantidadeEstoque": 1}, 201)
    carts = ["/api/carrinhos/" + api("POST", "/api/carrinhos", expected=201)["id"] for _ in range(2)]
    for cart_path in carts:
        api("POST", cart_path + "/itens", {**item, "quantidade": 1})
    with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:
        results = list(pool.map(lambda path: request("POST", path + "/checkout", expected=(200, 409, 422)), carts))
    assert sum(status == 200 for _, status in results) == 1, results
    assert api("GET", p)["quantidadeEstoque"] == 0
    winner = carts[next(i for i, (_, status) in enumerate(results) if status == 200)]
    for method, path, body in [
        ("POST", winner + "/itens", item),
        ("PUT", winner + f"/itens/{product_id}", {"quantidade": 1}),
        ("DELETE", winner + f"/itens/{product_id}", None),
        ("PUT", winner + "/cupom", {"codigoCupom": "10OFF"}),
        ("DELETE", winner + "/cupom", None),
        ("DELETE", winner, None),
        ("POST", winner + "/checkout", None),
    ]:
        api(method, path, body, 409)
    assert api("GET", winner)["status"] == "Finalizado"
    assert len(api("GET", "/api/carrinhos?pagina=1&tamanho=1")) == 1
    api("GET", "/api/carrinhos?pagina=0", expected=422)
    api("GET", "/api/carrinhos/" + str(uuid.uuid4()), expected=404)
    print(f"OK: {count} verificacoes HTTP, CRUD, calculos, erros, checkout e concorrencia.")


if __name__ == "__main__":
    run()
