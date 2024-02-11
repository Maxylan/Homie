# (c) 2024 @Maxylan
from typing import Optional
from sqlmodel import SQLModel, Field, ForeignKey

"""
The `Platform` model is a representation of the platforms table in the database.

It represents a single platform that is registered in Homie. A platform is 
its own private environment within Homie, you could say it's like a Home.
"""
class Platform(SQLModel, table=True):
    id: Optional[int] = Field(default=None, primary_key=True)
    slug: str = Field(max_length=63, unique=True)
    code: str = Field(max_length=63, unique=True)
    name: str = Field(max_length=63)
    master_pswd: str = Field(max_length=63)
    reset_token: str = Field(max_length=63)

    class Config:
        table_name = "platforms"

"""
The `PlatformConfig` model is a representation of the platform_configs table in the database.

It represents a single platform configuration, aka "Setting", unique to this platform.
"""
class PlatformConfig(SQLModel, table=True):
    id: Optional[int] = Field(default=None, primary_key=True)
    platform_id: int = Field(foreign_key="platform.id")
    key: str = Field(max_length=63)
    value: str = Field(max_length=255)

    class Config:
        table_name = "platform_configs"
        indexes = [("platform_id_index", ["platform_id"])]

"""
The `PlatformToken` model is a representation of the platform_tokens table in the database.

It represents a single user of a platform.
"""
class PlatformToken(SQLModel, table=True):
    id: Optional[int] = Field(default=None, primary_key=True)
    platform_id: int = Field(foreign_key="platform.id")
    name: str = Field(max_length=63)
    token: str = Field(max_length=63)
    expiry: Optional[str] = None

    class Config:
        table_name = "platform_tokens"
        indexes = [("name_index", ["name"]), ("token_index", ["token"])]

"""
GPT 3.5, 2024-02-11: In this representation:

Each table is represented by a class inheriting from SQLModel.
Each column is represented by a class attribute.
Additional meta-information like max length, uniqueness constraints, and indexes are provided using the Field class parameters such as max_length, unique, and indexes. Foreign keys are defined using the ForeignKey class.
Foreign key constraints are established using the foreign_key parameter within the Field class.
Indexes are defined within the Config class using the indexes attribute.
"""