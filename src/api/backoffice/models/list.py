# (c) 2024 @Maxylan
from typing import Optional
from sqlmodel import SQLModel, Field, ForeignKey

class List(SQLModel, table=True):
    id: Optional[int] = Field(default=None, primary_key=True)
    slug: str = Field(max_length=255, not_null=True)
    platform_id: int = Field(foreign_key="platforms.id", not_null=True)
    title: str = Field(max_length=255, not_null=True)
    created_by: Optional[int] = Field(foreign_key="platform_tokens.id", default=None)
    created: str = Field(default="CURRENT_TIMESTAMP", not_null=True)
    updated: str = Field(default="CURRENT_TIMESTAMP", not_null=True)
    locked: bool = Field(default=False, not_null=True)
    locked_by: Optional[int] = Field(foreign_key="platform_tokens.id", default=None)
    cover: Optional[int] = Field(foreign_key="attachments.id", default=None)
    data: Optional[str] = None

    class Config:
        table_name = "lists"
        indexes = [("created_index", ["created", "created_by"]), ("locked_index", ["locked", "locked_by"]), ("title_index", ["title", "slug"])]

class ListGroup(SQLModel, table=True):
    id: Optional[int] = Field(default=None, primary_key=True)
    list_id: int = Field(foreign_key="lists.id", not_null=True)
    platform_id: int = Field(foreign_key="platforms.id", not_null=True)
    title: str = Field(max_length=255, not_null=True)
    color: Optional[str] = Field(max_length=63, default=None)
    cover: Optional[int] = Field(foreign_key="attachments.id", default=None)
    data: Optional[str] = None

    class Config:
        table_name = "list_groups"
        indexes = [("title_index", ["title", "list_id"])]

"""
GPT 3.5, 2024-02-11: In this representation:

The lists table includes various constraints such as NOT NULL, DEFAULT, and foreign key relationships.
Indexes are established for efficient querying based on created date and created_by user, locked status and locked_by user, and the title of the list along with its slug.
Foreign key constraints are established using the foreign_key parameter within the Field class.
Timestamp columns default to the current timestamp and are specified as not nullable.
"""
