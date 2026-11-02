plugins {
    alias(libs.plugins.kotlinMultiplatform)
    alias(libs.plugins.androidMultiplatformLibrary)
    alias(libs.plugins.composeMultiplatform)
    alias(libs.plugins.composeCompiler)
    alias(libs.plugins.android.lint)
}

compose.resources {
    packageOfResClass = "com.dc.melodiasmario.core.commonui.designsystem.generated.resources"
    publicResClass = true
}

kotlin {
    android {
        namespace = "com.dc.melodiasmario.core.commonui"
        compileSdk { version = release(36) { minorApiLevel = 1 } }
        minSdk = 24

        androidResources { enable = true }
        withHostTestBuilder { }
        withDeviceTestBuilder { sourceSetTreeName = "test" }.configure {
            instrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"
        }
    }

    val xcfName = "uiKit"
    iosArm64 { binaries.framework { baseName = xcfName } }
    iosSimulatorArm64 { binaries.framework { baseName = xcfName } }

    sourceSets {
        commonMain {
            dependencies {
                implementation(dependencyNotation = project(":core:common"))
                api(dependencyNotation = project(":core:presentation"))
                implementation(dependencyNotation = project(":core:model"))

                implementation(dependencyNotation = libs.kotlin.stdlib)
                implementation(dependencyNotation = libs.compose.runtime)
                implementation(dependencyNotation = libs.compose.foundation)
                implementation(dependencyNotation = libs.compose.material3)
                implementation(dependencyNotation = libs.compose.ui)
                implementation(dependencyNotation = libs.compose.components.resources)
                implementation(dependencyNotation = libs.compose.uiToolingPreview)
                implementation(dependencyNotation = libs.coil.compose)
                implementation(dependencyNotation = libs.coil.network.ktor3)
            }
        }

        commonTest { dependencies { implementation(dependencyNotation = libs.kotlin.test) } }
        androidMain {
            dependencies {
                implementation(dependencyNotation = libs.compose.uiTooling)
                implementation(dependencyNotation = libs.core)
                implementation(dependencyNotation = libs.androidx.biometric)
                implementation(dependencyNotation = libs.androidx.fragment)
                implementation(dependencyNotation = libs.androidx.lifecycle.runtimeCompose)
            }
        }
        getByName("androidDeviceTest") {
            dependencies {
                implementation(dependencyNotation = libs.androidx.core)
                implementation(dependencyNotation = libs.androidx.junit)
                implementation(dependencyNotation = libs.androidx.runner)
            }
        }
    }
}
