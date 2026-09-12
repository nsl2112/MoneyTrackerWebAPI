from typing import List, Optional, Tuple, Dict, Any
from enum import Enum
import pandas as pd
from vnstock import Reference, Market

from core import (
    settings,
    cache,
    NotFoundException,
    VnstockProviderException,
)

from models import (
    FundListItemModel,
    FundNavHistoryItem,
    FundTopHoldingItem,
    FundAssetHoldingItem,
    FundIndustryHoldingItem,
    FundOverviewModel,
    FundNavQueryParams,
)

class FundType(str, Enum):
    EQUITY = "Quỹ Cổ phiếu"
    BALANCED = "Quỹ Cân bằng"
    BOND = "Quỹ Trái phiếu"

class FundService:
    """
    Dịch vụ quản lý và tra cứu thông tin các quỹ đầu tư mở tại Việt Nam (Fmarket).
    Tích hợp kiểm tra tính hợp lệ của mã quỹ, lọc dữ liệu và bộ nhớ đệm.
    """


    def __init__(self):
        self._ref = Reference()
        self._market = Market()
  
    def _get_raw_fund_list_df(self, fresh: bool = False) -> pd.DataFrame:
        """
        Lấy DataFrame danh sách quỹ gốc từ vnstock với cơ chế cache.
        """
        cache_key = "fund:raw_list_df"
        if not fresh:
            cached_df = cache.get(cache_key)
            if cached_df is not None:
                return cached_df

        try:
            df = self._ref.fund.list()
        except Exception as exc:
            raise VnstockProviderException(
                message="Không thể kết nối đến nguồn dữ liệu quỹ mở Fmarket",
                details={"error": str(exc)},
            )

        if df is None or df.empty:
            raise NotFoundException(message="Danh sách quỹ mở hiện đang trống hoặc không khả dụng")

        # Lưu cache danh sách thô
        cache.set(cache_key, df, ttl=settings.CACHE_FUND_LIST_TTL)
        return df

    def get_funds(
        self,
        fund_type: Optional[FundType] = None,
        search: Optional[str] = None,
        fresh: bool = False,
    ) -> Tuple[List[FundListItemModel], bool]:
        """
        Lấy danh sách các quỹ mở hỗ trợ lọc theo loại quỹ và tìm kiếm từ khóa.
        Trả về: (danh_sách_quỹ, is_cached).
        """
        cache_key = f"fund:list:{fund_type}:{search}"
        if not fresh:
            cached_result = cache.get(cache_key)
            if cached_result is not None:
                return cached_result, True

        df = self._get_raw_fund_list_df(fresh=fresh).copy()

        # Lọc theo loại quỹ nếu có
        if fund_type:
            df = df[df["fund_type"].astype(str).str.upper() == fund_type.value.strip().upper()]

        # Tìm kiếm theo mã quỹ hoặc tên quỹ
        if search:
            query = search.strip().lower()
            df = df[df["short_name"].astype(str).str.lower().str.contains(query)]

        # Chuẩn hóa giá trị NaN
        df = df.fillna("")

        items: List[FundListItemModel] = []
        for row in df.to_dict(orient="records"):
            # Chuyển đổi các trường số an toàn
            items.append(
                FundListItemModel(
                    short_name=row.get("short_name", ""),
                    name=row.get("name", ""),
                    fund_type=row.get("fund_type") or None,
                    fund_owner_name=row.get("fund_owner_name") or None,
                    management_fee=float(row.get("management_fee")) if row.get("management_fee") != "" else None,
                    inception_date=str(row.get("inception_date")) if row.get("inception_date") else None,
                    nav=float(row.get("nav")) if row.get("nav") != "" else None,
                    nav_change_previous=float(row.get("nav_change_previous")) if row.get("nav_change_previous") != "" else None,
                    nav_change_1m=float(row.get("nav_change_1m")) if row.get("nav_change_1m") != "" else None,
                    nav_change_3m=float(row.get("nav_change_3m")) if row.get("nav_change_3m") != "" else None,
                    nav_change_6m=float(row.get("nav_change_6m")) if row.get("nav_change_6m") != "" else None,
                    nav_change_12m=float(row.get("nav_change_12m")) if row.get("nav_change_12m") != "" else None,
                    nav_update_at=str(row.get("nav_update_at")) if row.get("nav_update_at") else None,
                )
            )

        cache.set(cache_key, items, ttl=settings.CACHE_FUND_LIST_TTL)
        return items, False

    def validate_fund_code(self, symbol: str) -> str:
        """
        Kiểm tra mã quỹ có nằm trong danh mục các quỹ đang niêm yết không.
        Trả về mã viết tắt in hoa nếu hợp lệ, ngược lại ném ngoại lệ NotFoundException.
        """
        code = symbol.strip().upper()
        df = self._get_raw_fund_list_df()
        valid_codes = set(df["short_name"].astype(str).str.upper().values)
        if code not in valid_codes:
            raise NotFoundException(
                message=f"Quỹ mở với mã '{code}' không tồn tại trên hệ thống",
                details={"provided_code": symbol, "available_count": len(valid_codes)},
            )
        return code

    def get_fund_nav_history(
        self,
        symbol: str,
        params: FundNavQueryParams,
    ) -> Tuple[List[FundNavHistoryItem], bool]:
        """
        Lấy lịch sử biến động NAV của quỹ theo các tiêu chí thời gian và giới hạn số lượng.
        """
        code = self.validate_fund_code(symbol)
        cache_key = f"fund:nav:{code}:{params.start}:{params.end}:{params.limit}"

        if not params.fresh:
            cached_data = cache.get(cache_key)
            if cached_data is not None:
                return cached_data, True

        try:
            df = self._market.fund(code).history()
        except Exception as exc:
            raise VnstockProviderException(
                message=f"Lỗi khi tải lịch sử NAV cho quỹ {code}",
                details={"fund_code": code, "error": str(exc)},
            )

        if df is None or df.empty:
            return [], False

        # Sao chép và sắp xếp theo ngày tăng dần
        df = df.copy()
        df["date"] = df["date"].astype(str)
        df = df.sort_values(by="date", ascending=True)

        # Lọc theo khoảng ngày
        if params.start:
            df = df[df["date"] >= params.start]
        if params.end:
            df = df[df["date"] <= params.end]

        # Lấy limit phiên gần nhất nếu được chỉ định
        if params.limit and len(df) > params.limit:
            df = df.tail(params.limit)

        records = df.to_dict(orient="records")
        nav_list = [
            FundNavHistoryItem(
                date=str(row["date"]),
                nav_per_unit=float(row["nav_per_unit"]),
            )
            for row in records
        ]

        cache.set(cache_key, nav_list, ttl=settings.CACHE_FUND_NAV_TTL)
        return nav_list, False

    def get_fund_overview(
        self, 
        symbol: str, 
        fresh: bool = False
    ) -> Tuple[FundOverviewModel, bool]:
        """
        Lấy thông tin tổng quan toàn diện về quỹ: thông tin chung, NAV mới nhất,
        top cổ phiếu nắm giữ, phân bổ tài sản và phân bổ ngành.
        """
        code = self.validate_fund_code(symbol)
        cache_key = f"fund:overview:{code}"

        if not fresh:
            cached_data = cache.get(cache_key)
            if cached_data is not None:
                return cached_data, True

        # 1. Lấy thông tin cơ bản của quỹ từ danh sách
        funds, _ = self.get_funds(search=code, fresh=fresh)
        fund_info = next((f for f in funds if f.short_name.upper() == code), None)
        if not fund_info:
            raise NotFoundException(message=f"Không tìm thấy thông tin cơ bản cho quỹ {code}")

        # 2. Lấy NAV mới nhất
        latest_nav: Optional[FundNavHistoryItem] = None
        try:
            nav_history, _ = self.get_fund_nav_history(code, FundNavQueryParams(limit=1, fresh=fresh))
            if nav_history:
                latest_nav = nav_history[-1]
        except Exception:
            latest_nav = None

        # 3. Lấy Top Holdings
        top_holdings: List[FundTopHoldingItem] = []
        try:
            th_df = self._market.fund(code).top_holding()
            if th_df is not None and not th_df.empty:
                th_df = th_df.fillna("")
                for row in th_df.to_dict(orient="records"):
                    top_holdings.append(
                        FundTopHoldingItem(
                            stock_code=row.get("stock_code") or None,
                            industry=row.get("industry") or None,
                            net_asset_percent=float(row.get("net_asset_percent")) if row.get("net_asset_percent") != "" else None,
                            type_asset=row.get("type_asset") or None,
                            update_at=str(row.get("update_at")) if row.get("update_at") else None,
                        )
                    )
        except Exception:
            pass

        # 4. Lấy Phân bổ tài sản (Asset Allocation)
        asset_allocations: List[FundAssetHoldingItem] = []
        try:
            ah_df = self._market.fund(code).asset_holding()
            if ah_df is not None and not ah_df.empty:
                ah_df = ah_df.fillna("")
                for row in ah_df.to_dict(orient="records"):
                    asset_allocations.append(
                        FundAssetHoldingItem(
                            asset_type=row.get("asset_type") or None,
                            asset_percent=float(row.get("asset_percent")) if row.get("asset_percent") != "" else None,
                        )
                    )
        except Exception:
            pass

        # 5. Lấy Phân bổ ngành (Industry Allocation)
        industry_allocations: List[FundIndustryHoldingItem] = []
        try:
            ih_df = self._market.fund(code).industry_holding()
            if ih_df is not None and not ih_df.empty:
                ih_df = ih_df.fillna("")
                for row in ih_df.to_dict(orient="records"):
                    industry_allocations.append(
                        FundIndustryHoldingItem(
                            industry=row.get("industry") or None,
                            net_asset_percent=float(row.get("net_asset_percent")) if row.get("net_asset_percent") != "" else None,
                        )
                    )
        except Exception:
            pass

        overview = FundOverviewModel(
            info=fund_info,
            latest_nav=latest_nav,
            top_holdings=top_holdings,
            asset_allocation=asset_allocations,
            industry_allocation=industry_allocations,
        )

        cache.set(cache_key, overview, ttl=settings.CACHE_FUND_HOLDING_TTL)
        return overview, False


fund_service = FundService()
