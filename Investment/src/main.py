from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from fastapi.exceptions import RequestValidationError

from core import settings, cache
from core.errors import (
    AppException,
    app_exception_handler,
    validation_exception_handler,
    global_exception_handler,
)
from routers import index_router, fund_router

app = FastAPI(
    title=settings.PROJECT_NAME,
    version=settings.VERSION,
    description=(
        "API tra cứu dữ liệu Chỉ số chứng khoán (VNINDEX, VN30, HNX, UPCOM) "
        "và Giá trị tài sản ròng (NAV), danh mục các quỹ mở tại thị trường chứng khoán Việt Nam. "
        "Hỗ trợ bộ nhớ đệm (In-Memory Cache) và thiết kế mở rộng khung thời gian (Interval)."
    ),
    docs_url="/docs",
    redoc_url="/redoc",
)

# Cấu hình CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Đăng ký các bộ xử lý ngoại lệ tập trung
app.add_exception_handler(AppException, app_exception_handler)
app.add_exception_handler(RequestValidationError, validation_exception_handler)
app.add_exception_handler(Exception, global_exception_handler)

# Gắn các routers API v1
app.include_router(index_router, prefix=settings.API_V1_STR)
app.include_router(fund_router, prefix=settings.API_V1_STR)


@app.get("/", tags=["Hệ thống (System)"])
async def root():
    """
    Trang chào mừng và chỉ mục API.
    """
    return {
        "project": settings.PROJECT_NAME,
        "version": settings.VERSION,
        "docs": "/docs",
        "api_v1": settings.API_V1_STR,
        "endpoints": {
            "indices": f"{settings.API_V1_STR}/indices",
            "funds": f"{settings.API_V1_STR}/funds",
        },
    }


@app.get("/health", tags=["Hệ thống (System)"])
async def health_check():
    """
    Kiểm tra trạng thái sẵn sàng của dịch vụ API và hiệu suất cache.
    """
    return {
        "status": "healthy",
        "cache_stats": cache.stats(),
    }


@app.post("/cache/clear", tags=["Hệ thống (System)"])
async def clear_cache():
    """
    Dọn dẹp thủ công toàn bộ bộ nhớ đệm hệ thống.
    """
    cache.clear()
    return {"message": "Đã làm sạch toàn bộ bộ nhớ đệm (Cache cleared)"}
