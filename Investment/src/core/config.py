import os
from pathlib import Path
from pydantic_settings import BaseSettings, SettingsConfigDict
import vnai

# Đường dẫn tới thư mục gốc của dự án
BASE_DIR = Path(__file__).resolve().parent.parent.parent
ENV_FILE = BASE_DIR / ".env"


class Settings(BaseSettings):
    """
    Lớp quản lý cấu hình hệ thống và môi trường.
    Tự động nạp các biến môi trường từ tệp .env.
    """
    PROJECT_NAME: str = "Vietnam Stock & Fund Market API"
    VERSION: str = "1.0.0"
    API_V1_STR: str = "/api/v1"
    
    # Khóa API Vnstock
    VNSTOCK_API_KEY: str = ""
    
    # Cấu hình bộ nhớ đệm (thời gian sống tính bằng giây)
    CACHE_INDEX_OHLCV_TTL: int = 300        # 5 phút cho nến chỉ số
    CACHE_INDEX_LATEST_TTL: int = 60        # 1 phút cho điểm số tức thời
    CACHE_INDEX_LIST_TTL: int = 86400       # 24 giờ cho danh mục chỉ số
    CACHE_FUND_LIST_TTL: int = 43200        # 12 giờ cho danh sách quỹ mở
    CACHE_FUND_NAV_TTL: int = 7200          # 2 giờ cho lịch sử NAV quỹ
    CACHE_FUND_HOLDING_TTL: int = 86400     # 24 giờ cho danh mục nắm giữ của quỹ
    
    model_config = SettingsConfigDict(
        env_file=str(ENV_FILE),
        env_file_encoding="utf-8",
        extra="ignore"
    )


settings = Settings()

# Thiết lập khóa API cho thư viện vnstock / vnai nếu có
if settings.VNSTOCK_API_KEY:
    try:
        vnai.setup_api_key(settings.VNSTOCK_API_KEY)
    except Exception as exc:
        print(f"[Cảnh báo] Không thể kích hoạt VNSTOCK_API_KEY: {exc}")
