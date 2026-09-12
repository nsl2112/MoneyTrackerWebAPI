from typing import List, Annotated
from fastapi import APIRouter, Query, Path

from models import ApiResponse
from models import (
    IndexInfoModel,
    IndexCandleModel,
    IndexLatestModel,
    IndexQueryParams,
)
from services import index_service

router = APIRouter(prefix="/indices", tags=["Chỉ số thị trường (Market Indices)"])


@router.get(
    "",
    response_model=ApiResponse[List[IndexInfoModel]],
    summary="Danh sách các chỉ số thị trường được hỗ trợ",
    description="Trả về danh sách các chỉ số chứng khoán chính tại Việt Nam (VNINDEX, VN30, HNX, UPCOM, VN100...).",
)
async def get_supported_indices():
    indices, is_cached = index_service.get_supported_indices()
    return ApiResponse(
        data=indices,
        message="Lấy danh sách chỉ số thị trường thành công",
        cached=is_cached,
    )


@router.get(
    "/{code}/ohlcv",
    response_model=ApiResponse[List[IndexCandleModel]],
    summary="Lịch sử nến (OHLCV) của chỉ số",
    description=(
        "Lấy chuỗi dữ liệu nến Open, High, Low, Close, Volume theo khoảng thời gian và khung nến (interval). "
        "Mặc định là '1D'. Có thể thiết lập tham số fresh=true để bỏ qua cache."
    ),
)
async def get_index_ohlcv(
    code: Annotated[str, Path(description="Mã chỉ số (ví dụ: VNINDEX, VN30, HNXIndex, UPCOMIndex)")],
    query: Annotated[IndexQueryParams, Query()],
):
    candles, is_cached = index_service.get_index_ohlcv(symbol=code, params=query)
    return ApiResponse(
        data=candles,
        cached=is_cached,
        message=f"Lấy {len(candles)} nến cho chỉ số {code.upper()} thành công",
    )


@router.get(
    "/{code}/latest",
    response_model=ApiResponse[IndexLatestModel],
    summary="Điểm số phiên giao dịch gần nhất của chỉ số",
    description="Lấy điểm số, khối lượng và tỷ lệ thay đổi của phiên giao dịch gần nhất.",
)
async def get_index_latest(
    code: Annotated[str, Path(description="Mã chỉ số (ví dụ: VNINDEX, VN30)")],
    fresh: Annotated[bool, Query(description="Bỏ qua cache để cập nhật dữ liệu mới")] = False,
):
    latest_data, is_cached = index_service.get_index_latest(symbol=code, fresh=fresh)
    return ApiResponse(
        data=latest_data,
        cached=is_cached,
        message=f"Lấy điểm số mới nhất của chỉ số {code.upper()} thành công",
    )
