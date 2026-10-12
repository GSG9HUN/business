package com.dc.melodiasmario.playback

import android.graphics.Bitmap
import android.graphics.BitmapFactory
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import java.net.HttpURLConnection
import java.net.URL

class PlaybackArtworkLoader {
    private var cachedUrl: String? = null
    private var cachedBitmap: Bitmap? = null

    suspend fun loadArtWork(url: String?): Bitmap? {
        if (url.isNullOrBlank()){
            clear()
            return null
        }
        if (url == cachedUrl && cachedBitmap != null){
            return cachedBitmap
        }
        return withContext(Dispatchers.IO) {
            runCatching {
                val connection = URL(url).openConnection() as HttpURLConnection
                connection.connectTimeout = CONNECT_TIMEOUT_MILLIS
                connection.readTimeout = READ_TIMEOUT_MILLIS
                connection.instanceFollowRedirects = true

                connection.inputStream.use(BitmapFactory::decodeStream)
            }.getOrNull()
        }.also { bitmap ->
            cachedUrl = url
            cachedBitmap = bitmap
        }
    }

    fun clear() {
        cachedUrl = null
        cachedBitmap = null
    }

    private companion object {
        const val CONNECT_TIMEOUT_MILLIS = 3000
        const val READ_TIMEOUT_MILLIS = 5000
    }
}