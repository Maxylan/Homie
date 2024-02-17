# (c) 2024 @Maxylan
from ctypes import Union
from main import Homie
from ...models.homiedb.platform import Platform
from fastapi import Response

prefix = "/platforms"

class iPlatformController:
    def __init__(self):
        pass

    async def get_all_platforms(self) -> list[Platform]:
        raise NotImplementedError

    async def get_platform(self, platform_id: int) -> Platform:
        raise NotImplementedError

    async def create_platform(self, platform: Platform) -> Platform:
        raise NotImplementedError

    async def update_platform(self, platform_id: int, platform: Platform) -> Platform:
        raise NotImplementedError

    async def delete_platform(self, platform_id: int) -> Response:
        raise NotImplementedError