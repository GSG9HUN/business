package com.dc.melodiasmario.core.commonui.biometric

import androidx.compose.runtime.Composable
import androidx.compose.runtime.remember
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.Res
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.biometric_prompt_reason
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.biometric_prompt_title
import com.dc.melodiasmario.core.presentation.biometric.BiometricResult
import org.jetbrains.compose.resources.stringResource

/**
 * Remembers biometric authentication for the current composition.
 */
@Composable
fun rememberBiometricAuthentication(
    biometricAuthenticator: BiometricAuthenticator = localBiometricAuthenticator.current
): suspend () -> BiometricResult {

    val title = stringResource(resource = Res.string.biometric_prompt_title)
    val reason = stringResource(resource = Res.string.biometric_prompt_reason)

    return remember(key1 = biometricAuthenticator, key2 = title, key3 = reason) {
        suspend {
            biometricAuthenticator.authenticate(
                title = title,
                reason = reason
            )
        }
    }
}
