from enum import Enum
from typing import Generic, TypeVar, Optional, Any
from pydantic import BaseModel, Field

T = TypeVar("T")


class IntervalEnum(str, Enum):
    """
    Các khung thời gian nến (Interval).
    Mặc định là 1D (ngày). Được thiết kế sẵn sàng mở rộng các khung khác.
    """
    ONE_DAY = "1D"
    ONE_WEEK = "1W"
    ONE_MONTH = "1M"
    # Khung thời gian trong ngày (Intraday) - dự phòng mở rộng
    ONE_HOUR = "1H"
    THIRTY_MINUTES = "30m"
    FIFTEEN_MINUTES = "15m"
    FIVE_MINUTES = "5m"
    ONE_MINUTE = "1m"


class ApiResponse(BaseModel, Generic[T]):
    """
    Định dạng phản hồi API chuẩn hóa toàn hệ thống.
    """
    success: bool = Field(True, description="Trạng thái thành công của yêu cầu")
    message: Optional[str] = Field(None, description="Thông điệp bổ sung nếu có")
    data: Optional[T] = Field(None, description="Dữ liệu kết quả")
    cached: bool = Field(False, description="Dữ liệu có được lấy từ bộ nhớ đệm hay không")
