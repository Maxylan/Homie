# (c) 2024 @Maxylan
from typing import Optional
from sqlmodel import Field, SQLModel
from enum import Enum

class HttpMethod(str, Enum):
    GET = 'GET'
    PUT = 'PUT'
    POST = 'POST'
    DELETE = 'DELETE'
    OPTIONS = 'OPTIONS'
    HEAD = 'HEAD'
    PATCH = 'PATCH'
    UNKNOWN = 'UNKNOWN'

"""
The `AccessLog` model is a representation of the access_logs table in the database. 
It represents a single request proxied by the Reverse Proxy.
"""
class AccessLog(SQLModel, table = True):
    id: Optional[int] = Field(default = None, primary_key = True)
    platform_id: Optional[int] = None
    uid: Optional[int] = None
    timestamp: Optional[str] = Field(default = None, sql_default = "CURRENT_TIMESTAMP")
    ip: str = Field(max_length=63)
    method: HttpMethod = Field(default = HttpMethod.UNKNOWN, max_length = 8)
    uri: str = Field(max_length=127)
    path: str = Field(max_length=255)
    parameters: str = Field(max_length=255)
    full_url: str = Field(max_length=511)
    headers: str = Field(max_length=1023)
    body: Optional[str] = None
    response: Optional[str] = None
    response_status: Optional[int] = None

    class Config:
        table_name = "access_logs"

        # Index definitions
        indexes = [
            ("ip_index", ["ip"]),
            ("method_index", ["method"])
        ]

"""
GPT 3.5, 2024-02-11: In this representation:

The access_logs table includes various constraints such as NOT NULL and the primary key.
Indexes are established for efficient querying based on ip and method.
The method column is defined as an Enum to restrict values to a predefined set of options.
Foreign key constraints are commented out for flexibility.
"""