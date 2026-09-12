import datetime
from typing import Optional, Any
from pydantic import BaseModel, Field, field_validator, model_validator
from .common_models import IntervalEnum


class IndexInfoModel(BaseModel):
    """
    Thông tin định danh cơ bản của một chỉ số thị trường.
    """
    code: str = Field(..., description="Mã chỉ số (ví dụ: VNINDEX, VN30, HNX)")
    name: str = Field(..., description="Tên đầy đủ của chỉ số")
    exchange: str = Field(..., description="Sàn giao dịch liên kết (HOSE, HNX, UPCOM)")
    description: Optional[str] = Field(None, description="Mô tả tóm tắt về chỉ số")


class IndexCandleModel(BaseModel):
    """
    Dữ liệu nến giao dịch lịch sử (OHLCV) của chỉ số.
    """
    time: datetime.datetime = Field(..., description="Thời gian nến giao dịch")
    open: float = Field(..., description="Giá mở cửa")
    high: float = Field(..., description="Giá cao nhất")
    low: float = Field(..., description="Giá thấp nhất")
    close: float = Field(..., description="Giá đóng cửa")
    volume: float = Field(..., description="Khối lượng giao dịch")


class IndexLatestModel(BaseModel):
    """
    Thông tin nến hoặc điểm số phiên giao dịch gần nhất của chỉ số.
    """
    code: str = Field(..., description="Mã chỉ số")
    time: datetime.datetime = Field(..., description="Thời gian cập nhật phiên gần nhất")
    close: float = Field(..., description="Điểm số đóng cửa phiên gần nhất")
    open: float = Field(..., description="Điểm số mở cửa")
    high: float = Field(..., description="Điểm số cao nhất phiên")
    low: float = Field(..., description="Điểm số thấp nhất phiên")
    volume: float = Field(..., description="Khối lượng khớp lệnh")
    change: Optional[float] = Field(None, description="Mức thay đổi điểm so với phiên trước")
    change_percent: Optional[float] = Field(None, description="Tỷ lệ phần trăm thay đổi (%)")


class IndexQueryParams(BaseModel):
    """
    Các tham số truy vấn dữ liệu chỉ số.
    """
    start: Optional[str] = Field(
        None,
        description="Ngày bắt đầu (định dạng YYYY-MM-DD). Mặc định lấy 30 ngày trước."
    )
    end: Optional[str] = Field(
        None,
        description="Ngày kết thúc (định dạng YYYY-MM-DD). Mặc định là ngày hôm nay."
    )
    interval: IntervalEnum = Field(
        default=IntervalEnum.ONE_DAY,
        description="Khung thời gian nến (mặc định '1D')."
    )
    count: Optional[int] = Field(
        default=100,
        ge=1,
        le=5000,
        description="Số lượng nến tối đa cần lấy nếu không chỉ định ngày bắt đầu."
    )
    fresh: bool = Field(
        default=False,
        description="Bỏ qua bộ nhớ đệm (cache) để lấy dữ liệu mới nhất từ nguồn."
    )

    @model_validator(mode="before")
    @classmethod
    def validate_dates(cls, values: Any) -> Any:
        if isinstance(values, dict):
            start = values.get("start")
            end = values.get("end")
            date_format = "%Y-%m-%d"
            d_start, d_end = None, None

            if start:
                try:
                    d_start = datetime.datetime.strptime(start, date_format).date()
                except ValueError:
                    raise ValueError(f"Định dạng ngày bắt đầu không hợp lệ: '{start}'. Yêu cầu YYYY-MM-DD.")

            if end:
                try:
                    d_end = datetime.datetime.strptime(end, date_format).date()
                except ValueError:
                    raise ValueError(f"Định dạng ngày kết thúc không hợp lệ: '{end}'. Yêu cầu YYYY-MM-DD.")

            if d_start and d_end and d_start > d_end:
                raise ValueError("Ngày bắt đầu (start) không được lớn hơn ngày kết thúc (end).")

        return values
