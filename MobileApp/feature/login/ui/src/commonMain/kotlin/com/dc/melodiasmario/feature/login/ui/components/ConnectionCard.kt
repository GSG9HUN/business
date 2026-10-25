package com.dc.melodiasmario.feature.login.ui.components

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Card
import androidx.compose.material3.CardDefaults
import androidx.compose.material3.MaterialTheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.dc.melodiasmario.core.model.login.ApiConnectionStatus
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MBadge
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.Res
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.api_available_detail
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.api_available_title
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.api_unavailable_detail
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.api_unavailable_title
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.connection_section_title
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.offline_badge
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.online_badge
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioTheme
import org.jetbrains.compose.resources.stringResource

@Composable
fun ConnectionCard(
    apiConnectionStatus: ApiConnectionStatus,
    modifier: Modifier = Modifier,
) {
    val isOnline = apiConnectionStatus == ApiConnectionStatus.Online
    val statusBackground = if (isOnline) MaterialTheme.colorScheme.tertiaryContainer else MaterialTheme.colorScheme.errorContainer
    val statusTextColor = if (isOnline) MaterialTheme.colorScheme.onTertiaryContainer else MaterialTheme.colorScheme.onErrorContainer

    Card(
        modifier = modifier.fillMaxWidth(),
        shape = RoundedCornerShape(14.dp),
        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
        border = BorderStroke(1.dp, MaterialTheme.colorScheme.outline),
    ) {
        Column(
            modifier = Modifier.padding(14.dp),
        ) {
            MText(
                text = stringResource(Res.string.connection_section_title),
                color = MaterialTheme.colorScheme.onSurfaceVariant,
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
                        color = MaterialTheme.colorScheme.onSurface,
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
                        color = MaterialTheme.colorScheme.onSurfaceVariant,
                        fontSize = 11.sp,
                    )
                }

                MBadge(
                    text = stringResource(
                        if (isOnline) Res.string.online_badge else Res.string.offline_badge,
                    ),
                    backgroundColor = statusBackground,
                    contentColor = statusTextColor,
                )
            }
        }
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
