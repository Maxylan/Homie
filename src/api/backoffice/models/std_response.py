# (c) 2024 @Maxylan
from http.client import HTTPResponse
from typing import Any, Optional
from pydantic import BaseModel
from enum import Enum
import json

class HttpMethod(str, Enum):
    GET = 'GET'
    PUT = 'PUT'
    POST = 'POST'
    DELETE = 'DELETE'
    OPTIONS = 'OPTIONS'
    HEAD = 'HEAD'
    PATCH = 'PATCH'
    UNKNOWN = 'UNKNOWN'


# Standard Responses, still on the fence if I should use these.
    
class ErrorResult(BaseModel):
    def __init__(self, message: Optional[str] = None, data: Optional[dict] = None):
        if message is not None:
            message = message.strip()
            if message != "" and message != "None":
                self.message = message

        self.data = data

    message: Optional[str] = None
    data: Optional[dict] = None

class HomieResponse(BaseModel):
    def __init__(self, result: BaseModel|ErrorResult, meta: Optional[dict] = None):
        self.result = result
        self.meta = meta

    result: BaseModel|ErrorResult
    meta: Optional[dict] = None