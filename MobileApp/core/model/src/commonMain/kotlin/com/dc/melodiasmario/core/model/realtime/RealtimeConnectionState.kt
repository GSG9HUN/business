package com.dc.melodiasmario.core.model.realtime

/**
 * Represents realtime connection state shared between application layers.
 */
enum class RealtimeConnectionState {
    Disconnected,
    Connecting,
    Connected,
    Reconnecting
}
