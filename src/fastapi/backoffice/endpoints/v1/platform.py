# (c) 2024 @Maxylan
from ..definitions.platform import iPlatformController
from ...models.homiedb.platform import Platform
from main import homie
from fastapi import Response, HTTPException

prefix = "/platforms"

class PlatformControllerV1(iPlatformController):
    def __init__(self):
        pass

    @homie.get(prefix)
    async def get_all_platforms(self) -> list[Platform]:
        raise HTTPException(500, "NotImplementedException")

    @homie.get(prefix + '/{platform_id}')
    async def get_platform(self, platform_id: int) -> Platform:
        return {}

    async def create_platform(self, platform: Platform) -> Platform:
        raise HTTPException(500, "NotImplementedException")

    async def update_platform(self, platform_id: int, platform: Platform) -> Platform:
        raise HTTPException(500, "NotImplementedException")

    async def delete_platform(self, platform_id: int) -> Response:
        raise HTTPException(500, "NotImplementedException")
    
