package com.dc.melodiasmario.core.commonui.biometric

import androidx.compose.runtime.Composable
import androidx.compose.ui.tooling.preview.Preview
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.Res
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.biometric_not_authenticated
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.biometric_storage_failure
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.biometric_unavailable
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.presentation.biometric.AppLockError
import org.jetbrains.compose.resources.stringResource

/**
 * Provides app lock error text for the current composition.
 */
@Composable
fun appLockErrorText(
    error: AppLockError?
): String? = when (error) {
    AppLockError.NotAuthenticated -> {
        stringResource(resource = Res.string.biometric_not_authenticated)
    }

    AppLockError.AuthenticationUnavailable -> {
        stringResource(resource = Res.string.biometric_unavailable)
    }

    AppLockError.StorageFailure -> {
        stringResource(resource = Res.string.biometric_storage_failure)
    }

    null -> null
}
@Preview(showBackground = true)
@Composable
private fun AppLockErrorTextPreview() {
    MelodiasMarioTheme {
        appLockErrorText(error = AppLockError.AuthenticationUnavailable)?.let { errorText ->
            MText(text = errorText)
        }
    }
}
