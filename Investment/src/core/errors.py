from fastapi import Request, status
from fastapi.responses import JSONResponse
from fastapi.exceptions import RequestValidationError


class AppException(Exception):
    """
    Ngoại lệ cơ sở cho toàn bộ ứng dụng.
    """
    def __init__(self, message: str, status_code: int = 400, details: dict = None):
        super().__init__(message)
        self.message = message
        self.status_code = status_code
        self.details = details or {}


class NotFoundException(AppException):
    """
    Lỗi không tìm thấy tài nguyên (Mã chỉ số hoặc Quỹ không tồn tại).
    """
    def __init__(self, message: str = "Tài nguyên không tìm thấy", details: dict = None):
        super().__init__(message, status_code=status.HTTP_404_NOT_FOUND, details=details)


class VnstockProviderException(AppException):
    """
    Lỗi khi kết nối hoặc xử lý dữ liệu từ nguồn cung cấp vnstock.
    """
    def __init__(self, message: str = "Lỗi khi lấy dữ liệu từ nhà cung cấp", details: dict = None):
        super().__init__(message, status_code=status.HTTP_502_BAD_GATEWAY, details=details)


async def app_exception_handler(request: Request, exc: AppException) -> JSONResponse:
    """
    Xử lý tập trung các ngoại lệ nghiệp vụ của ứng dụng.
    """
    return JSONResponse(
        status_code=exc.status_code,
        content={
            "success": False,
            "error": {
                "message": exc.message,
                "status_code": exc.status_code,
                "details": exc.details,
            }
        },
    )


async def validation_exception_handler(request: Request, exc: RequestValidationError) -> JSONResponse:
    """
    Xử lý tập trung các lỗi kiểm tra tính hợp lệ dữ liệu (Pydantic validation).
    """
    errors = []
    for err in exc.errors():
        loc = " -> ".join([str(l) for l in err.get("loc", [])])
        msg = err.get("msg", "Tham số không hợp lệ")
        errors.append(f"{loc}: {msg}")

    return JSONResponse(
        status_code=status.HTTP_422_UNPROCESSABLE_ENTITY,
        content={
            "success": False,
            "error": {
                "message": "Dữ liệu yêu cầu không hợp lệ",
                "status_code": 422,
                "details": {"validation_errors": errors},
            }
        },
    )


async def global_exception_handler(request: Request, exc: Exception) -> JSONResponse:
    """
    Bắt các lỗi bất ngờ chưa được phân loại để tránh làm crash app.
    """
    return JSONResponse(
        status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
        content={
            "success": False,
            "error": {
                "message": "Lỗi máy chủ nội bộ. Vui lòng thử lại sau.",
                "status_code": 500,
                "details": {"raw_error": str(exc)},
            }
        },
    )
