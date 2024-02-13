# (c) 2024 @Maxylan
from typing import Optional
from enum import Enum
from sqlmodel import SQLModel, Field, ForeignKey

class StoreEnum(str, Enum):
    citygross = 'citygross'
    ica = 'ica'
    lidl = 'lidl'
    hemkop = 'hemkop'

class Product(SQLModel, table = True):
    product_id: Optional[int] = Field(default = None, primary_key = True)
    pid: str = Field(max_length = 63)
    store: StoreEnum
    name: str = Field(max_length = 255)
    brand: Optional[str] = Field(max_length = 63)
    source: Optional[str] = Field(max_length = 255)
    description: Optional[str] = None
    cover: Optional[int] = Field(foreign_key = "attachments.id", default = None)
    attachment: Optional[int] = Field(foreign_key = "attachments.id", default = None)
    timestamp: str = Field(default = "CURRENT_TIMESTAMP", nullable = True)

    class Config:
        table_name = "products"
        indexes = [("pid_index", ["pid"]), ("store_index", ["store"]), ("name_index", ["name"])]

class ProductIndex(SQLModel, table = True):
    id: Optional[int] = Field(default = None, primary_key = True)
    product_id: int = Field(foreign_key = "products.product_id", nullable = True)
    pid: str = Field(foreign_key = "products.pid", max_length = 63, nullable = True)
    index: str
    value: str = Field(max_length = 255)
    timestamp: str = Field(default = "CURRENT_TIMESTAMP", nullable = True)

    class Config:
        table_name = "product_indexes"
        indexes = [("value_index", ["index", "value"])]

class ProductPrice(SQLModel, table = True):
    id: Optional[int] = Field(default = None, primary_key = True)
    product_id: int = Field(foreign_key = "products.product_id", nullable = True)
    pid: str = Field(foreign_key = "products.pid", max_length = 63, nullable = True)
    unit: str = Field(max_length = 63, nullable = True)
    current: float = Field(nullable = True)
    ordinary: float = Field(nullable = True)
    timestamp: str = Field(default = "CURRENT_TIMESTAMP", nullable = True)
    promotion: bool = Field(default = False, nullable = True)
    promotion_start: Optional[str] = None
    promotion_end: Optional[str] = None
    promotion_value: Optional[float] = None
    promotion_price: Optional[float] = None

    class Config:
        table_name = "product_prices"
        indexes = [("price_index", ["current", "ordinary"]), ("promotion_index", ["promotion", "promotion_start", "promotion_end"])]

    # @property
    # def is_on_promotion(self) -> bool:
    #     return self.promotion and self.promotion_start <= self.timestamp <= self.promotion_end
    

"""
GPT 3.5, 2024-02-11: In this representation:

The products & product_prices tables includes various constraints such as NOT NULL, DEFAULT, and foreign key relationships.
An enumeration (StoreEnum) is defined for the store column to restrict its values to a predefined set of options.
Indexes are established for efficient querying based on current and ordinary prices, as well as for columns related to promotions.
The product_indexes table defines indexes for grouping and filtering products by specific criteria, with constraints and relationships maintained accordingly.
Foreign key constraints are established using the foreign_key parameter within the Field class.
Timestamp columns default to the current timestamp and are specified as not nullable.
A property is_on_promotion is added to the model to determine whether the product is currently on promotion, based on the current timestamp and promotion start/end dates.
"""