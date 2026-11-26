package com.dc.melodiasmario.feature.audioeffect.ui.components

import androidx.compose.foundation.layout.Row
import androidx.compose.material3.Checkbox
import androidx.compose.material3.LocalContentColor
import androidx.compose.material3.LocalTextStyle
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.tooling.preview.Preview
import com.dc.melodiasmario.core.commonui.designsystem.components.dialog.MConfirmDialog
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.components.input.MTextInputDialog
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeMode
import com.dc.melodiasmario.feature.audioeffect.generated.resources.Res
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_cancel
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_delete
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_delete_confirm
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_name_hint
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_overwrite
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_preset_name
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_save
import com.dc.melodiasmario.feature.audioeffect.presentation.AudioEffectsEvent
import org.jetbrains.compose.resources.stringResource

/** Renders preset save and delete dialogs without owning playback or navigation state. */
@Composable
internal fun AudioEffectsDialogs(
    showSaveDialog: Boolean,
    deletePreset: String?,
    onEvent: (event: AudioEffectsEvent) -> Unit,
    onSaveDismissed: () -> Unit,
    onDeleteDismissed: () -> Unit
) {
    if (showSaveDialog) {
        AudioSavePresetDialog(
            onDismiss = { onSaveDismissed() },
            onSave = { name, overwrite ->
                onSaveDismissed()
                onEvent(AudioEffectsEvent.SavePreset(name = name, overwrite = overwrite))
            }
        )
    }

    deletePreset?.let { presetName ->
        MConfirmDialog(
            title = stringResource(resource = Res.string.audio_effects_delete),
            message = stringResource(
                resource = Res.string.audio_effects_delete_confirm,
                formatArgs = arrayOf<Any>(presetName)
            ),
            confirmText = stringResource(resource = Res.string.audio_effects_delete),
            dismissText = stringResource(resource = Res.string.audio_effects_cancel),
            onConfirm = {
                onDeleteDismissed()
                onEvent(AudioEffectsEvent.DeletePreset(name = presetName))
            },
            onDismiss = onDeleteDismissed,
            isDestructive = true
        )
    }
}

/** Requests a valid guild preset name and explicit overwrite consent. */
@Composable
private fun AudioSavePresetDialog(onDismiss: () -> Unit, onSave: (name: String, overwrite: Boolean) -> Unit) {
    var name by remember { mutableStateOf(value = "") }
    var overwrite by remember { mutableStateOf(value = false) }
    MTextInputDialog(
        title = stringResource(resource = Res.string.audio_effects_save),
        value = name,
        onValueChange = { inputValue -> name = inputValue.take(n = 32) },
        onDismiss = onDismiss,
        onConfirm = { onSave(name, overwrite) },
        placeholder = stringResource(resource = Res.string.audio_effects_preset_name),
        confirmText = stringResource(resource = Res.string.audio_effects_save),
        dismissText = stringResource(resource = Res.string.audio_effects_cancel),
        canConfirm = name.matches(regex = Regex(pattern = "[A-Za-z0-9_-]{1,32}")),
        supportingContent = {
            MText(
                text = stringResource(resource = Res.string.audio_effects_name_hint),
                style = LocalTextStyle.current,
                color = LocalContentColor.current,
                textAlign = LocalTextStyle.current.textAlign,
                lineHeight = LocalTextStyle.current.lineHeight
            )
            Row(verticalAlignment = Alignment.CenterVertically) {
                Checkbox(checked = overwrite, onCheckedChange = { checked -> overwrite = checked })
                MText(
                    text = stringResource(resource = Res.string.audio_effects_overwrite),
                    style = LocalTextStyle.current,
                    color = LocalContentColor.current,
                    textAlign = LocalTextStyle.current.textAlign,
                    lineHeight = LocalTextStyle.current.lineHeight
                )
            }
        }
    )
}

@Preview(showBackground = true)
@Composable
private fun AudioEffectsDeleteDialogPreview() {
    MelodiasMarioTheme(themeMode = MelodiasMarioThemeMode.Dark) {
        AudioEffectsDialogs(
            showSaveDialog = false,
            deletePreset = "Late Night",
            onEvent = {},
            onSaveDismissed = {},
            onDeleteDismissed = {}
        )
    }
}

@Preview(showBackground = true)
@Composable
private fun AudioEffectsSaveDialogPreview() {
    MelodiasMarioTheme(themeMode = MelodiasMarioThemeMode.Dark) {
        AudioSavePresetDialog(onDismiss = {}, onSave = { _, _ -> })
    }
}
