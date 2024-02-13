# (c) 2024 @Maxylan
from typing import Optional
from sqlmodel import SQLModel, Field, ForeignKey

class Message(SQLModel, table = True):
    id: Optional[int] = Field(default = None, primary_key = True)
    platform_id: int = Field(foreign_key = "platforms.id", nullable = True)
    message: str = Field(max_length = None)  # Text fields in SQLModel do not have a max length
    sent: str = Field(default = "CURRENT_TIMESTAMP", nullable = True)
    sent_by: Optional[int] = Field(foreign_key = "platform_tokens.id", default = None)
    attachment: Optional[int] = Field(foreign_key = "attachments.id", default = None)

    class Config:
        table_name = "messages"
        indexes = [("sent_index", ["sent"]), ("sent_by_index", ["sent_by"])]

"""
GPT 3.5, 2024-02-11: In this representation:

Foreign key constraints are established using the foreign_key parameter within the Field class.
Constraints such as NOT NULL and DEFAULT are provided using the nullable and default parameters within the Field class.
Indexes are defined within the Config class using the indexes attribute.
For the message field, since it's a TEXT type which can store large amounts of data, I didn't specify a max_length parameter as TEXT fields in SQLModel do not have a max length.
"""