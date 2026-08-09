package com.dc.melodiasmario.shared.ui.screen.auth.components

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Card
import androidx.compose.material3.CardDefaults
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.dc.melodiasmario.shared.domain.status.model.ApiConnectionStatus
import com.dc.melodiasmario.shared.ui.theme.MelodiasMarioTheme
import com.dc.melodiasmario.shared.ui.theme.MmStatusOfflineBg
import com.dc.melodiasmario.shared.ui.theme.MmStatusOfflineText
import com.dc.melodiasmario.shared.ui.theme.MmStatusOnlineBg
import com.dc.melodiasmario.shared.ui.theme.MmStatusOnlineText
import com.dc.melodiasmario.shared.ui.theme.MmSurface
import com.dc.melodiasmario.shared.ui.theme.MmSurfaceOutline
import com.dc.melodiasmario.shared.ui.theme.MmTextMuted
import com.dc.melodiasmario.shared.ui.theme.MmTextPrimary
import com.dc.melodiasmario.shared.ui.theme.MmTextSecondary
import com.dc.melodiasmario.shared.ui.common.MText
import org.jetbrains.compose.resources.stringResource
import mobileapp.shared.generated.resources.Res
import mobileapp.shared.generated.resources.api_available_detail
import mobileapp.shared.generated.resources.api_available_title
import mobileapp.shared.generated.resources.api_unavailable_detail
import mobileapp.shared.generated.resources.api_unavailable_title
import mobileapp.shared.generated.resources.connection_section_title
import mobileapp.shared.generated.resources.offline_badge
import mobileapp.shared.generated.resources.online_badge

@Composable
fun ConnectionCard(
    apiConnectionStatus: ApiConnectionStatus,
    modifier: Modifier = Modifier,
) {
    val isOnline = apiConnectionStatus == ApiConnectionStatus.Online
    val statusBackground = if (isOnline) MmStatusOnlineBg else MmStatusOfflineBg
    val statusTextColor = if (isOnline) MmStatusOnlineText else MmStatusOfflineText

    Card(
        modifier = modifier.fillMaxWidth(),
        shape = RoundedCornerShape(14.dp),
        colors = CardDefaults.cardColors(containerColor = MmSurface),
        border = BorderStroke(1.dp, MmSurfaceOutline),
    ) {
        Column(
            modifier = Modifier.padding(14.dp),
        ) {
            MText(
                text = stringResource(Res.string.connection_section_title),
                color = MmTextSecondary,
                fontSize = 12.sp,
                fontWeight = FontWeight.ExtraBold,
            )

            Spacer(modifier = Modifier.height(10.dp))

            Row(
                modifier = Modifier.fillMaxWidth(),
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.spacedBy(12.dp),
            ) {
                StatusIcon(
                    text = if (isOnline) "\u2713" else "\u00d7",
                    backgroundColor = statusBackground,
                    textColor = statusTextColor,
                )

                Column(
                    modifier = Modifier.weight(1f),
                ) {
                    MText(
                        text = stringResource(
                            if (isOnline) {
                                Res.string.api_available_title
                            } else {
                                Res.string.api_unavailable_title
                            },
                        ),
                        color = MmTextPrimary,
                        fontSize = 14.sp,
                        fontWeight = FontWeight.Bold,
                    )
                    MText(
                        text = stringResource(
                            if (isOnline) {
                                Res.string.api_available_detail
                            } else {
                                Res.string.api_unavailable_detail
                            },
                        ),
                        color = MmTextMuted,
                        fontSize = 11.sp,
                    )
                }

                StatusBadge(
                    text = stringResource(
                        if (isOnline) Res.string.online_badge else Res.string.offline_badge,
                    ),
                    backgroundColor = statusBackground,
                    textColor = statusTextColor,
                )
            }
        }
    }
}

@Composable
private fun StatusIcon(
    text: String,
    backgroundColor: Color,
    textColor: Color,
) {
    Box(
        modifier = Modifier
            .size(48.dp)
            .clip(RoundedCornerShape(12.dp))
            .background(backgroundColor),
        contentAlignment = Alignment.Center,
    ) {
        MText(
            text = text,
            color = textColor,
            fontSize = 22.sp,
            fontWeight = FontWeight.Bold,
        )
    }
}

@Composable
private fun StatusBadge(
    text: String,
    backgroundColor: Color,
    textColor: Color,
) {
    Box(
        modifier = Modifier
            .clip(RoundedCornerShape(999.dp))
            .background(backgroundColor)
            .padding(horizontal = 8.dp, vertical = 4.dp),
    ) {
        MText(
            text = text,
            color = textColor,
            fontSize = 10.sp,
            fontWeight = FontWeight.Bold,
        )
    }
}

@Preview
@Composable
private fun ConnectionCardOnlinePreview() {
    MelodiasMarioTheme {
        ConnectionCard(apiConnectionStatus = ApiConnectionStatus.Online)
    }
}

@Preview
@Composable
private fun ConnectionCardOfflinePreview() {
    MelodiasMarioTheme {
        ConnectionCard(apiConnectionStatus = ApiConnectionStatus.Offline)
    }
}
