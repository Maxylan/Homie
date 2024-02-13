# (c) 2024 @Maxylan
from main import Homie
from ..models.homiedb.platform import Platform
from fastapi import HTTPException

prefix = "/platforms"

@Homie.get(prefix)
async def get_all_platforms():
    raise HTTPException(500, "NotImplementedException")

@Homie.get(prefix + '/{platform_id}')
async def platform(platform_id: int):
    return {}