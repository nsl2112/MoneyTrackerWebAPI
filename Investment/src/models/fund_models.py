import datetime
from typing import Optional, List, Any
from pydantic import BaseModel, Field, model_validator


class FundListItemModel(BaseModel):
    """
    Thông tin tóm tắt của một quỹ mở trong danh sách.
    """
    short_name: str = Field(..., description="Mã viết tắt của quỹ (ví dụ: DCDS, VMEFF, VESAF)")
    name: str = Field(..., description="Tên đầy đủ của quỹ")
    fund_type: Optional[str] = Field(None, description="Phân loại quỹ (EQUITY, BALANCED, BOND...)")
    fund_owner_name: Optional[str] = Field(None, description="Công ty quản lý quỹ")
    management_fee: Optional[float] = Field(None, description="Phí quản lý hàng năm (%)")
    inception_date: Optional[str] = Field(None, description="Ngày thành lập quỹ")
    nav: Optional[float] = Field(None, description="Giá trị tài sản ròng (NAV/CCQ) hiện tại")
    nav_change_previous: Optional[float] = Field(None, description="Thay đổi NAV so với phiên trước (%)")
    nav_change_1m: Optional[float] = Field(None, description="Thay đổi NAV trong 1 tháng gần nhất (%)")
    nav_change_3m: Optional[float] = Field(None, description="Thay đổi NAV trong 3 tháng gần nhất (%)")
    nav_change_6m: Optional[float] = Field(None, description="Thay đổi NAV trong 6 tháng gần nhất (%)")
    nav_change_12m: Optional[float] = Field(None, description="Thay đổi NAV trong 12 tháng gần nhất (%)")
    nav_update_at: Optional[str] = Field(None, description="Ngày cập nhật NAV gần nhất")


class FundNavHistoryItem(BaseModel):
    """
    Điểm dữ liệu NAV của quỹ theo ngày.
    """
    date: str = Field(..., description="Ngày định giá NAV (YYYY-MM-DD)")
    nav_per_unit: float = Field(..., description="Giá trị NAV trên mỗi chứng chỉ quỹ")


class FundTopHoldingItem(BaseModel):
    """
    Khoản đầu tư / Cổ phiếu nắm giữ hàng đầu trong danh mục của quỹ.
    """
    stock_code: Optional[str] = Field(None, description="Mã chứng khoán")
    industry: Optional[str] = Field(None, description="Ngành nghề kinh doanh")
    net_asset_percent: Optional[float] = Field(None, description="Tỷ trọng trên tổng tài sản ròng (%)")
    type_asset: Optional[str] = Field(None, description="Loại tài sản (STOCK, BOND, CASH...)")
    update_at: Optional[str] = Field(None, description="Ngày cập nhật danh mục")


class FundAssetHoldingItem(BaseModel):
    """
    Phân bổ theo loại tài sản của quỹ.
    """
    asset_type: Optional[str] = Field(None, description="Tên nhóm tài sản (Cổ phiếu, Tiền mặt...)")
    asset_percent: Optional[float] = Field(None, description="Tỷ trọng tài sản (%)")


class FundIndustryHoldingItem(BaseModel):
    """
    Phân bổ danh mục theo ngành kinh tế.
    """
    industry: Optional[str] = Field(None, description="Tên nhóm ngành kinh tế")
    net_asset_percent: Optional[float] = Field(None, description="Tỷ trọng trên tổng giá trị ròng (%)")


class FundOverviewModel(BaseModel):
    """
    Bức tranh tổng quan toàn diện về quỹ mở.
    """
    info: FundListItemModel = Field(..., description="Thông tin cơ bản và hiệu suất của quỹ")
    latest_nav: Optional[FundNavHistoryItem] = Field(None, description="Giá trị NAV mới nhất")
    top_holdings: List[FundTopHoldingItem] = Field(default_factory=list, description="Top danh mục nắm giữ")
    asset_allocation: List[FundAssetHoldingItem] = Field(default_factory=list, description="Phân bổ loại tài sản")
    industry_allocation: List[FundIndustryHoldingItem] = Field(default_factory=list, description="Phân bổ theo ngành")


class FundNavQueryParams(BaseModel):
    """
    Tham số truy vấn lịch sử NAV quỹ mở.
    """
    start: Optional[str] = Field(None, description="Ngày bắt đầu lọc (YYYY-MM-DD)")
    end: Optional[str] = Field(None, description="Ngày kết thúc lọc (YYYY-MM-DD)")
    limit: Optional[int] = Field(None, ge=1, le=5000, description="Giới hạn số phiên gần nhất cần lấy")
    fresh: bool = Field(False, description="Bỏ qua cache để tải dữ liệu mới từ nguồn")

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
