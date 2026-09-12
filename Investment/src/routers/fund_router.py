from typing import List, Optional, Annotated
from fastapi import APIRouter, Query, Path

from models import ApiResponse
from models import (
    FundListItemModel,
    FundNavHistoryItem,
    FundOverviewModel,
    FundNavQueryParams,
)
from services import fund_service, FundType

router = APIRouter(prefix="/funds", tags=["Quỹ mở (Mutual Funds)"])


@router.get(
    "",
    response_model=ApiResponse[List[FundListItemModel]],
    summary="Danh sách các quỹ mở niêm yết (Fmarket)",
    description=(
        "Lấy danh sách toàn bộ các quỹ mở tại Việt Nam. "
        "Hỗ trợ lọc theo loại quỹ (EQUITY, BALANCED, BOND...) và tìm kiếm từ khóa theo mã hoặc tên quỹ."
    ),
)
async def list_funds(
    fund_type: Annotated[Optional[FundType], Query(description="Loại quỹ (BALANCED, EQUITY, BOND)")] = None,
    search: Annotated[Optional[str], Query(description="Từ khóa tìm kiếm theo mã hoặc tên quỹ")] = None,
    fresh: Annotated[bool, Query(description="Bỏ qua cache để tải lại danh sách mới nhất")] = False,
):
    funds, is_cached = fund_service.get_funds(fund_type=fund_type, search=search, fresh=fresh)
    return ApiResponse(
        data=funds,
        cached=is_cached,
        message=f"Tìm thấy {len(funds)} quỹ phù hợp",
    )


@router.get(
    "/{code}/nav",
    response_model=ApiResponse[List[FundNavHistoryItem]],
    summary="Lịch sử giá NAV chứng chỉ quỹ",
    description="Tra cứu chuỗi lịch sử giá trị tài sản ròng (NAV/CCQ) theo ngày của một quỹ cụ thể.",
)
async def get_fund_nav_history(
    code: Annotated[str, Path(description="Mã viết tắt của quỹ (ví dụ: DCDS, VMEFF, VESAF)")],
    query: Annotated[FundNavQueryParams, Query()],
):
    nav_items, is_cached = fund_service.get_fund_nav_history(symbol=code, params=query)
    return ApiResponse(
        data=nav_items,
        cached=is_cached,
        message=f"Lấy {len(nav_items)} bản ghi NAV cho quỹ {code.upper()} thành công",
    )


@router.get(
    "/{code}/overview",
    response_model=ApiResponse[FundOverviewModel],
    summary="Tổng quan chi tiết quỹ và danh mục đầu tư",
    description=(
        "Cung cấp thông tin đầy đủ về quỹ: Thông tin pháp lý, NAV mới nhất, "
        "Top cổ phiếu nắm giữ, phân bổ theo loại tài sản và phân bổ theo ngành."
    ),
)
async def get_fund_overview(
    code: Annotated[str, Path(description="Mã viết tắt của quỹ (ví dụ: DCDS, VMEFF)")],
    fresh: Annotated[bool, Query(description="Bỏ qua cache để tải dữ liệu mới")] = False,
):
    overview, is_cached = fund_service.get_fund_overview(symbol=code, fresh=fresh)
    return ApiResponse(
        data=overview,
        cached=is_cached,
        message=f"Lấy thông tin tổng quan quỹ {code.upper()} thành công",
    )
