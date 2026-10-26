package com.dc.melodiasmario.core.commonui.biometric

import androidx.compose.runtime.Composable
import androidx.compose.runtime.remember
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.*
import com.dc.melodiasmario.core.presentation.biometric.BiometricResult
import org.jetbrains.compose.resources.stringResource

@Composable
fun rememberBiometricAuthentication(
    authenticator: BiometricAuthenticator = LocalBiometricAuthenticator.current,
): suspend () -> BiometricResult {

    val title = stringResource(Res.string.biometric_prompt_title)
    val reason = stringResource(Res.string.biometric_prompt_reason)

    return remember(authenticator, title, reason) {
        suspend {
            authenticator.authenticate(
                title = title,
                reason = reason
            )
        }
    }
}