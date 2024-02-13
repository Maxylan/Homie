# (c) 2024 @Maxylan
from http.client import HTTPResponse
from typing import Any, Optional
from pydantic import BaseModel
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


class HomieResult(BaseModel):
    def __init__(self, response_or_data: Optional[HTTPResponse]|Optional[dict] = None, message: Optional[str] = None):
        if response_or_data is not None:
            if isinstance(response_or_data, HTTPResponse): 
                self.data = response_or_data.read().decode('utf-8')
            else: 
                self.data = response_or_data

        if message is not None and message.strip != "" and message != "None":
            self.message = message

    message: Optional[str] = None
    data: Optional[dict] = None

class HomieResponse(BaseModel):
    def __init__(self, response_or_result: Optional[HTTPResponse]|Optional[dict]|Optional[HomieResult] = None, message: Optional[str] = None):
        if isinstance(response_or_result, HomieResult):
            self.result = response_or_result
            return

        if isinstance(response_or_result, HTTPResponse): 
            self.status = response_or_result.status
        
        self.result = HomieResult(response_or_result, message)

    result: HomieResult
    status: Optional[int] = None 
    meta: Optional[dict] = None