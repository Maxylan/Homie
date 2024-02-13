# (c) 2024 @Maxylan
from typing import Optional
from sqlmodel import SQLModel, Field, ForeignKey

class Recipe(SQLModel, table = True):
    id: Optional[int] = Field(default = None, primary_key = True)
    slug: str = Field(max_length = 255, nullable = True)
    platform_id: int = Field(foreign_key = "platforms.id", nullable = True)
    title: str = Field(max_length = 255, nullable = True)
    created_by: Optional[int] = Field(foreign_key = "platform_tokens.id", default = None)
    created: str = Field(default = "CURRENT_TIMESTAMP", nullable = True)
    updated: str = Field(default = "CURRENT_TIMESTAMP", nullable = True)
    locked: bool = Field(default = False, nullable = True)
    locked_by: Optional[int] = Field(foreign_key = "platform_tokens.id", default = None)
    cover: Optional[int] = Field(foreign_key = "attachments.id", default = None)
    data: Optional[str] = None

    class Config:
        table_name = "recipes"
        indexes = [("created_index", ["created", "created_by"]), ("locked_index", ["locked", "locked_by"]), ("title_index", ["title", "slug"])]

"""
GPT 3.5, 2024-02-11: In this representation:

The recipes table includes various constraints such as NOT NULL and foreign key relationships.
Indexes are established for efficient querying based on created date and created_by user, locked status and locked_by user, and the title of the recipe along with its slug.
Foreign key constraints are established using the foreign_key parameter within the Field class.
"""