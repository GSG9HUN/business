package com.dc.melodiasmario.shared.ui.screen.auth

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.dc.melodiasmario.shared.domain.status.model.ApiConnectionStatus
import com.dc.melodiasmario.shared.presentation.auth.LoginEvent
import com.dc.melodiasmario.shared.presentation.auth.LoginUiState
import com.dc.melodiasmario.shared.ui.common.MText
import com.dc.melodiasmario.shared.ui.screen.auth.components.ConnectionCard
import com.dc.melodiasmario.shared.ui.screen.auth.components.MelodiasLogo
import com.dc.melodiasmario.shared.ui.theme.MelodiasMarioTheme
import com.dc.melodiasmario.shared.ui.theme.MmBackgroundPreviewColor
import com.dc.melodiasmario.shared.ui.theme.MmPrimary
import com.dc.melodiasmario.shared.ui.theme.MmSurface
import com.dc.melodiasmario.shared.ui.theme.MmSurfacePreviewColor
import com.dc.melodiasmario.shared.ui.theme.MmTextPrimary
import mobileapp.shared.generated.resources.Res
import mobileapp.shared.generated.resources.app_name
import mobileapp.shared.generated.resources.login_button
import mobileapp.shared.generated.resources.login_lead
import org.jetbrains.compose.resources.stringResource
@Composable
fun LoginScreen(
    uiState: LoginUiState,
    modifier: Modifier = Modifier,
    onEvent: (LoginEvent) -> Unit = {},
) {
    Surface(
        modifier = modifier.fillMaxSize(),
        color = MmSurface,
    ) {
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(horizontal = 24.dp),
            horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.Center,
        ) {
            MelodiasLogo()

            Spacer(modifier = Modifier.height(24.dp))

            MText(
                text = stringResource(Res.string.app_name),
                color = MmTextPrimary,
                fontSize = 28.sp,
                fontWeight = FontWeight.ExtraBold,
                textAlign = TextAlign.Center,
            )

            Spacer(modifier = Modifier.height(12.dp))

            MText(text = stringResource(Res.string.login_lead))

            Spacer(modifier = Modifier.height(28.dp))

            Button(
                onClick = { onEvent(LoginEvent.DiscordLoginClicked) },
                modifier = Modifier
                    .fillMaxWidth()
                    .height(52.dp),
                enabled = uiState.apiConnectionStatus == ApiConnectionStatus.Online,
                shape = RoundedCornerShape(12.dp),
                colors = ButtonDefaults.buttonColors(
                    containerColor = MmPrimary,
                    contentColor = MmTextPrimary,
                ),
            ) {
                MText(
                    text = stringResource(Res.string.login_button),
                    fontWeight = FontWeight.Bold,
                )
            }

            Spacer(modifier = Modifier.height(18.dp))

            ConnectionCard(apiConnectionStatus = uiState.apiConnectionStatus)
        }
    }
}

@Preview(showBackground = true, backgroundColor = MmSurfacePreviewColor)
@Composable
private fun LoginScreenPreviewOnline() {
    MelodiasMarioTheme {
        LoginScreen(
            uiState = LoginUiState(apiConnectionStatus = ApiConnectionStatus.Online),
        )
    }
}


@Preview(showBackground = true, backgroundColor = MmBackgroundPreviewColor)
@Composable
private fun LoginScreenPreviewOffline() {
    MelodiasMarioTheme {
        LoginScreen(
            uiState = LoginUiState(apiConnectionStatus = ApiConnectionStatus.Offline),
        )
    }
}