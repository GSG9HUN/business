# Bot Control Entities

This folder contains EF Core entities for the API-to-bot command bridge.

## Why This Folder Exists

The API cannot directly execute bot process behavior. It persists command rows that the bot worker can claim later. The entity in this folder is the EF representation of that table.

## Files

### BotControlCommandEntity.cs

Stores bot control commands written by the API and consumed by the bot worker.

## What The Entity Represents

A command row captures the target guild, requesting user, command type, processing state, timestamps, and failure information.

## Maintenance Notes

- Commands are persisted so the API and bot process can communicate through the database.
- Keep entity fields aligned with the command lifecycle in `BotControlCommandsRepository`.
