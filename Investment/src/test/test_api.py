import sys
from pathlib import Path

# Thêm thư mục gốc vào PYTHONPATH
sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from fastapi.testclient import TestClient
from ..main import app

client = TestClient(app)


def test_api():
    print("\n========== BẮT ĐẦU KIỂM THỬ TOÀN DIỆN WEB API ==========\n")

    # 1. Kiểm tra Root Endpoint
    print("[1] Kiểm tra GET / ...")
    res = client.get("/")
    assert res.status_code == 200, f"Thất bại: {res.text}"
    print(f"    -> OK: {res.json()['project']}")

    # 2. Kiểm tra Health Endpoint
    print("[2] Kiểm tra GET /health ...")
    res = client.get("/health")
    assert res.status_code == 200, f"Thất bại: {res.text}"
    print(f"    -> OK: status = {res.json()['status']}, cache = {res.json()['cache_stats']}")

    # 3. Kiểm tra danh sách chỉ số
    print("[3] Kiểm tra GET /api/v1/indices ...")
    res = client.get("/api/v1/indices")
    assert res.status_code == 200, f"Thất bại: {res.text}"
    indices = res.json()["data"]
    print(f"    -> OK: Tìm thấy {len(indices)} chỉ số hỗ trợ (ví dụ: {[i['code'] for i in indices[:3]]})")

    # 4. Kiểm tra nến lịch sử VNINDEX
    print("[4] Kiểm tra GET /api/v1/indices/VNINDEX/ohlcv?count=5 ...")
    res1 = client.get("/api/v1/indices/VNINDEX/ohlcv?count=5")
    assert res1.status_code == 200, f"Thất bại: {res1.text}"
    data1 = res1.json()
    print(f"    -> OK: Nhận {len(data1['data'])} nến (cached={data1['cached']})")
    assert data1["cached"] is False, "Lần gọi đầu tiên không được cached"

    # 5. Kiểm tra Cache nến VNINDEX (lần 2 phải cached = True)
    print("[5] Kiểm tra Cache nến VNINDEX (lần gọi thứ 2) ...")
    res2 = client.get("/api/v1/indices/VNINDEX/ohlcv?count=5")
    assert res2.status_code == 200, f"Thất bại: {res2.text}"
    data2 = res2.json()
    print(f"    -> OK: Nhận {len(data2['data'])} nến (cached={data2['cached']})")
    assert data2["cached"] is True, "Lần gọi thứ 2 phải được lấy từ cache"

    # 6. Kiểm tra điểm số mới nhất VNINDEX
    print("[6] Kiểm tra GET /api/v1/indices/VNINDEX/latest ...")
    res = client.get("/api/v1/indices/VNINDEX/latest")
    assert res.status_code == 200, f"Thất bại: {res.text}"
    latest = res.json()["data"]
    print(f"    -> OK: {latest['code']} lúc {latest['time']}: Đóng cửa = {latest['close']}, Thay đổi = {latest['change']} ({latest['change_percent']}%)")

    # 7. Kiểm tra Alias chỉ số (HNX -> HNXIndex)
    print("[7] Kiểm tra Alias chỉ số HNX -> HNXIndex ...")
    res = client.get("/api/v1/indices/HNX/ohlcv?count=3")
    assert res.status_code == 200, f"Thất bại: {res.text}"
    print(f"    -> OK: Alias HNX hoạt động chính xác, nhận {len(res.json()['data'])} nến")

    # 8. Kiểm tra danh sách Quỹ mở
    print("[8] Kiểm tra GET /api/v1/funds ...")
    res = client.get("/api/v1/funds")
    assert res.status_code == 200, f"Thất bại: {res.text}"
    funds = res.json()["data"]
    print(f"    -> OK: Tìm thấy {len(funds)} quỹ mở niêm yết trên Fmarket")

    # 9. Kiểm tra tìm kiếm quỹ theo từ khóa
    print("[9] Kiểm tra GET /api/v1/funds?search=DCDS ...")
    res = client.get("/api/v1/funds?search=DCDS")
    assert res.status_code == 200, f"Thất bại: {res.text}"
    found = res.json()["data"]
    assert len(found) >= 1, "Phải tìm thấy quỹ DCDS"
    print(f"    -> OK: Tìm thấy quỹ {found[0]['short_name']} - {found[0]['name']}")

    # 10. Kiểm tra lịch sử NAV của quỹ DCDS
    print("[10] Kiểm tra GET /api/v1/funds/DCDS/nav?limit=5 ...")
    res1 = client.get("/api/v1/funds/DCDS/nav?limit=5")
    assert res1.status_code == 200, f"Thất bại: {res1.text}"
    data1 = res1.json()
    print(f"     -> OK: Nhận {len(data1['data'])} điểm dữ liệu NAV (cached={data1['cached']})")
    if data1["data"]:
        print(f"        Điểm NAV gần nhất: ngày {data1['data'][-1]['date']} = {data1['data'][-1]['nav_per_unit']}")

    # 11. Kiểm tra Cache NAV của quỹ DCDS (lần 2 phải cached = True)
    print("[11] Kiểm tra Cache NAV quỹ DCDS (lần gọi thứ 2) ...")
    res2 = client.get("/api/v1/funds/DCDS/nav?limit=5")
    assert res2.status_code == 200, f"Thất bại: {res2.text}"
    data2 = res2.json()
    print(f"     -> OK: cached={data2['cached']}")
    assert data2["cached"] is True, "Lần gọi thứ 2 phải được lấy từ cache"

    # 12. Kiểm tra Tổng quan Quỹ DCDS (Overview: info, top holdings, asset, industry)
    print("[12] Kiểm tra GET /api/v1/funds/DCDS/overview ...")
    res = client.get("/api/v1/funds/DCDS/overview")
    assert res.status_code == 200, f"Thất bại: {res.text}"
    ov = res.json()["data"]
    print(f"     -> OK: Quỹ {ov['info']['short_name']}")
    print(f"        - Top holdings: {len(ov['top_holdings'])} mã (ví dụ: {[h['stock_code'] for h in ov['top_holdings'][:3]]})")
    print(f"        - Phân bổ tài sản: {[a['asset_type'] for a in ov['asset_allocation']]}")
    print(f"        - Phân bổ ngành: {len(ov['industry_allocation'])} ngành")

    # 13. Kiểm tra xử lý lỗi khi nhập mã quỹ không tồn tại (404)
    print("[13] Kiểm tra xử lý mã quỹ không hợp lệ (404 Not Found) ...")
    res = client.get("/api/v1/funds/KHONGTONTAI/nav")
    assert res.status_code == 404, f"Mong đợi 404 nhưng nhận: {res.status_code}"
    err = res.json()
    print(f"     -> OK: Phản hồi chuẩn 404: {err['error']['message']}")

    # 14. Thống kê Cache cuối cùng
    print("[14] Kiểm tra thống kê cache cuối cùng ...")
    res = client.get("/health")
    print(f"     -> Thống kê Cache: {res.json()['cache_stats']}")

    print("\n========== TẤT CẢ 14 KIỂM THỬ ĐỀU ĐẠT CHUẨN (PASS) ==========\n")


if __name__ == "__main__":
    test_api()
