package com.dc.melodiasmario.core.commonui.biometric

import androidx.compose.ui.tooling.preview.Preview
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioTheme
import androidx.compose.runtime.Composable
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.*
import com.dc.melodiasmario.core.presentation.biometric.AppLockError
import org.jetbrains.compose.resources.stringResource

@Composable
fun appLockErrorText(
    error: AppLockError?
): String? = when (error) {
    AppLockError.NotAuthenticated -> {
        stringResource(Res.string.biometric_not_authenticated)
    }

    AppLockError.AuthenticationUnavailable -> {
        stringResource(Res.string.biometric_unavailable)
    }

    AppLockError.StorageFailure -> {
        stringResource(Res.string.biometric_storage_failure)
    }

    null -> null
}
@Preview(showBackground = true)
@Composable
private fun AppLockErrorTextPreview() {
    MelodiasMarioTheme {
        appLockErrorText(AppLockError.AuthenticationUnavailable)?.let {
            MText(text = it)
        }
    }
}
