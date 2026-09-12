# Kế Hoạch Triển Khai Web API (FastAPI & vnstock)

## 1. Thông Tin Môi Trường & Gói Tài Khoản vnstock

- **Gói tài khoản nhận diện**: **Community (Phiên bản cộng đồng có API Key)**
  - Giới hạn: **60 requests/phút**, **3600 requests/giờ**.
  - Thư viện sử dụng: `vnstock` v4.0.7 (các Layer chính: `Market`, `Reference`).
  - *Ý nghĩa đối với kiến trúc*: Giới hạn 60 requests/phút đòi hỏi **In-Memory Caching** đóng vai trò cực kỳ quan trọng để tránh tình trạng API bị nghẽn (HTTP 429) khi người dùng hoặc frontend gửi nhiều request liên tục.

---

## 2. Kiến Trúc Tổng Thể Hệ Thống (Modular Architecture)

Cấu trúc thư mục được thiết kế theo mô hình phân tầng chuẩn của FastAPI:

```text
src/
├── main.py                     # Khởi tạo FastAPI app, middleware (CORS), đăng ký routers
├── program.py                  # Entrypoint chạy trực tiếp & hỗ trợ tương thích ngược
├── test_api.py                 # Bộ kiểm thử tự động 14 kịch bản cho API & Cache
├── core/
│   ├── config.py               # Quản lý cấu hình (đọc .env, API key, cache TTL)
│   ├── cache.py                # Bộ nhớ đệm In-Memory TTLCache (Thread-safe)
│   └── errors.py               # Chuẩn hóa lỗi (Exception handlers, error responses)
├── models/
│   ├── __init__.py             # Export các models
│   ├── common_models.py        # Các enum & response chuẩn (IntervalEnum, ApiResponse)
│   ├── index_models.py         # Schemas cho Index (Request, OHLCV candle, Latest snapshot)
│   └── fund_models.py          # Schemas cho Fund (FundList, NAV, Holdings, Overview)
├── services/
│   ├── index_service.py        # Logic lấy dữ liệu Index từ Market().index() + Caching
│   └── fund_service.py         # Logic lấy dữ liệu Quỹ từ Market().fund() & Reference().fund + Caching
└── routers/
    ├── __init__.py
    ├── index_router.py         # Endpoints: /api/v1/indices
    └── fund_router.py          # Endpoints: /api/v1/funds
```

---

## 3. Chi Tiết Thiết Kế Các Endpoints

### 3.1. Nhóm Chỉ Số Thị Trường (`/api/v1/indices`)

1. **`GET /api/v1/indices`**:
   - **Mô tả**: Trả về danh sách các chỉ số thị trường phổ biến được hỗ trợ (VNINDEX, VN30, HNX, UPCOM, VN100...).
   - **Cache**: 24 giờ.
2. **`GET /api/v1/indices/{code}/ohlcv`**:
   - **Mô tả**: Lấy dữ liệu lịch sử nến (Open, High, Low, Close, Volume).
   - **Query Parameters**:
     - `start`: Ngày bắt đầu (`YYYY-MM-DD`).
     - `end`: Ngày kết thúc (`YYYY-MM-DD`).
     - `interval`: Khung thời gian (`IntervalEnum`). Mặc định hiện tại là `'1D'`. Thiết kế sẵn cấu trúc enum để sẵn sàng hỗ trợ `'1H'`, `'15m'`, `'5m'`, `'1m'` khi người dùng nâng cấp gói.
     - `count`: Số lượng nến tối đa cần lấy nếu không truyền start/end.
     - `fresh`: Bỏ qua cache để lấy dữ liệu mới nhất.
   - **Cache**: 5–15 phút trong giờ giao dịch, 4 giờ ngoài giờ giao dịch.
3. **`GET /api/v1/indices/{code}/latest`**:
   - **Mô tả**: Lấy giá đóng cửa, khối lượng và tỷ lệ thay đổi phiên gần nhất của chỉ số.
   - **Cache**: 1–5 phút.

---

### 3.2. Nhóm Quỹ Mở (`/api/v1/funds`)

1. **`GET /api/v1/funds`**:
   - **Mô tả**: Lấy danh sách toàn bộ các quỹ mở đang giao dịch trên thị trường (Fmarket).
   - **Query Parameters (tùy chọn)**: `fund_type` (Cổ phiếu, Trái phiếu, Cân bằng), `search` (tìm theo mã hoặc tên).
   - **Dữ liệu trả về**: Mã quỹ (`short_name`), tên quỹ, loại quỹ, công ty quản lý, NAV hiện tại, hiệu suất 1M/6M/1Y/3Y.
   - **Cache**: 12 giờ.
2. **`GET /api/v1/funds/{code}/nav`**:
   - **Mô tả**: Lấy lịch sử biến động NAV/chứng chỉ quỹ của một quỹ cụ thể.
   - **Query Parameters**:
     - `start`: Ngày bắt đầu (`YYYY-MM-DD`).
     - `end`: Ngày kết thúc (`YYYY-MM-DD`).
     - `limit`: Số phiên gần nhất cần lấy (mặc định lấy toàn bộ hoặc theo giới hạn chỉ định).
     - `fresh`: Bỏ qua cache để tải dữ liệu mới từ nguồn.
   - **Cache**: 1–2 giờ (do NAV quỹ mở tại Việt Nam chỉ chốt một lần vào cuối ngày).
3. **`GET /api/v1/funds/{code}/overview`**:
   - **Mô tả**: Thông tin chi tiết toàn diện của quỹ:
     - Thông tin pháp lý & phí quản lý.
     - Điểm NAV mới nhất.
     - Top cổ phiếu/trái phiếu nắm giữ (`top_holdings`).
     - Phân bổ theo loại tài sản (`asset_allocation`).
     - Phân bổ theo ngành (`industry_allocation`).
   - **Cache**: 24 giờ.

---

## 4. Thiết Kế Bộ Nhớ Đệm (In-Memory TTLCache)

- **Cơ chế**: Triển khai class `InMemoryTTLCache` chuẩn Thread-safe (`threading.RLock`) với cơ chế tự động giải phóng bản ghi hết hạn (Time-To-Live) và dọn dẹp bộ nhớ (eviction).
- **TTL mặc định theo danh mục**:
  - `INDEX_LIST`: 86400 giây (24h)
  - `INDEX_OHLCV`: 300 giây (5 phút)
  - `INDEX_LATEST`: 60 giây (1 phút)
  - `FUND_LIST`: 43200 giây (12h)
  - `FUND_NAV`: 7200 giây (2h)
  - `FUND_HOLDINGS`: 86400 giây (24h)
- Cho phép người dùng bypass cache bằng query parameter `fresh=true` khi cần thiết.

---

## 5. Trạng Thái Triển Khai (Action Steps)

- [x] **Bước 1**: Khởi tạo `src/core/config.py` và nạp tự động `VNSTOCK_API_KEY` từ `.env`.
- [x] **Bước 2**: Xây dựng module `src/core/cache.py` với cơ chế TTLCache thread-safe.
- [x] **Bước 3**: Chuẩn hóa Pydantic Schemas trong `src/models/`:
  - `common_models.py`: Khai báo `IntervalEnum` (chứa `'1D'`, `'1W'`, `'1M'` và đặt nền móng cho `'1H'`, `'15m'`, `'5m'`, `'1m'`).
  - `index_models.py` & `fund_models.py`.
- [x] **Bước 4**: Xây dựng các Service Layer trong `src/services/`:
  - `index_service.py` với `Market().index(...)` + caching.
  - `fund_service.py` với `Reference().fund` và `Market().fund(...)` + caching.
- [x] **Bước 5**: Xây dựng các Routers trong `src/routers/` và gắn vào `src/main.py`.
- [x] **Bước 6**: Kiểm thử toàn diện API (End-to-end testing) bằng `src/test_api.py` (14/14 tests Passed).
