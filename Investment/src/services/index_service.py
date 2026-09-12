import datetime
from typing import List, Optional, Dict, Tuple
import pandas as pd
from vnstock import Market, Reference

from core import (
    settings,
    cache,
    NotFoundException,
    VnstockProviderException
)

from models import (
    IntervalEnum,
    IndexInfoModel,
    IndexCandleModel,
    IndexLatestModel,
    IndexQueryParams,
)

class IndexService:
    """
    Dịch vụ xử lý dữ liệu các chỉ số chứng khoán Việt Nam (VNINDEX, VN30, HNX, UPCOM...).
    Tích hợp bộ nhớ đệm (In-Memory TTLCache) và xử lý dữ liệu vector hóa.
    """

    def __init__(self):
        self._market = Market()
        self._reference = Reference()

    def normalize_symbol(self, symbol: str) -> str:
        """
        Chuẩn hóa mã chỉ số thành định dạng hệ thống hỗ trợ.
        """
        return symbol.strip().upper()

    def get_supported_indices(self) -> Tuple[List[IndexInfoModel], bool]:
        """
        Lấy danh sách thông tin các chỉ số thị trường được hỗ trợ.
        """
        cache_key = "index:supported_list"
        cached_data = cache.get(cache_key)
        if cached_data is not None:
            return cached_data, True

        df_indecies = self._reference.index.list()
        index_records = df_indecies.to_dict(orient="records")

        result = [
            IndexInfoModel(
                code=record["symbol"],
                name=record["full_name"],
                exchange=record["group"],
                description=record["description"],
            )
            for record in index_records
        ]
        cache.set(cache_key, result, ttl=settings.CACHE_INDEX_LIST_TTL)
        return result, False

    def get_index_ohlcv(
        self,
        symbol: str,
        params: IndexQueryParams,
    ) -> Tuple[List[IndexCandleModel], bool]:
        """
        Lấy dữ liệu nến lịch sử (OHLCV) của một chỉ số theo khoảng thời gian và interval.
        Trả về tuple: (danh_sách_nến, is_cached).
        """
        norm_code = self.normalize_symbol(symbol)
        interval_val = params.interval.value

        cache_key = (
            f"index:ohlcv:{norm_code}:{params.start}:{params.end}:{interval_val}:{params.count}"
        )

        if not params.fresh:
            cached_data = cache.get(cache_key)
            if cached_data is not None:
                return cached_data, True

        try:
            # Gọi API vnstock thông qua Market().index
            kwargs = {"interval": interval_val}
            if params.start:
                kwargs["start"] = params.start
            if params.end:
                kwargs["end"] = params.end
            if params.count and not (params.start and params.end):
                kwargs["count"] = params.count

            df = self._market.index(norm_code).ohlcv(**kwargs)
        except Exception as exc:
            raise VnstockProviderException(
                message=f"Không thể tải dữ liệu nến cho chỉ số {norm_code}",
                details={"symbol": norm_code, "error": str(exc)},
            )

        if df is None or df.empty:
            raise NotFoundException(
                message=f"Không tìm thấy dữ liệu giao dịch cho chỉ số {norm_code} trong khoảng thời gian đã chọn",
                details={"symbol": norm_code, "params": params.model_dump()},
            )

        # Xử lý vector hóa bằng Pandas
        df = df.copy()
        if "time" in df.columns:
            df["time"] = pd.to_datetime(df["time"])
        df = df.fillna(0)

        # Chuyển đổi sang Pydantic models
        records = df.to_dict(orient="records")
        candles = [IndexCandleModel(**row) for row in records]

        # Lưu vào cache
        cache.set(cache_key, candles, ttl=settings.CACHE_INDEX_OHLCV_TTL)
        return candles, False

    def get_index_latest(self, symbol: str, fresh: bool = False) -> Tuple[IndexLatestModel, bool]:
        """
        Lấy thông tin và điểm số phiên giao dịch gần nhất của chỉ số.
        Trả về tuple: (IndexLatestModel, is_cached).
        """
        norm_code = self.normalize_symbol(symbol)
        cache_key = f"index:latest:{norm_code}"

        if not fresh:
            cached_data = cache.get(cache_key)
            if cached_data is not None:
                return cached_data, True

        try:
            # Lấy 2 phiên gần nhất để tính mức chênh lệch điểm và %
            df = self._market.index(norm_code).ohlcv(count=2)
        except Exception as exc:
            raise VnstockProviderException(
                message=f"Không thể tải điểm số mới nhất cho chỉ số {norm_code}",
                details={"symbol": norm_code, "error": str(exc)},
            )

        if df is None or df.empty:
            raise NotFoundException(
                message=f"Không tìm thấy dữ liệu cho chỉ số {norm_code}",
                details={"symbol": norm_code},
            )

        latest_row = df.iloc[-1]
        change, change_percent = None, None

        if len(df) >= 2:
            prev_row = df.iloc[-2]
            prev_close = float(prev_row["close"])
            curr_close = float(latest_row["close"])
            change = round(curr_close - prev_close, 2)
            if prev_close != 0:
                change_percent = round((change / prev_close) * 100, 2)

        result = IndexLatestModel(
            code=norm_code,
            time=pd.to_datetime(latest_row["time"]),
            close=float(latest_row["close"]),
            open=float(latest_row["open"]),
            high=float(latest_row["high"]),
            low=float(latest_row["low"]),
            volume=float(latest_row["volume"]),
            change=change,
            change_percent=change_percent,
        )

        cache.set(cache_key, result, ttl=settings.CACHE_INDEX_LATEST_TTL)
        return result, False


index_service = IndexService()
