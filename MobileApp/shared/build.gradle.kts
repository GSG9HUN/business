import org.jetbrains.kotlin.gradle.dsl.JvmTarget

plugins {
    alias(libs.plugins.kotlinMultiplatform)
    alias(libs.plugins.androidMultiplatformLibrary)
    alias(libs.plugins.composeMultiplatform)
    alias(libs.plugins.composeCompiler)
    alias(libs.plugins.jetbrains.kotlin.serialization)
    alias(libs.plugins.ksp)
    alias(libs.plugins.koin.compiler)
}

compose.resources {
    packageOfResClass = "com.dc.melodiasmario.shared.generated.resources"
    publicResClass = true
}

kotlin {
    listOf(
        iosArm64(),
        iosSimulatorArm64()
    ).forEach { iosTarget ->
        iosTarget.binaries.framework {
            baseName = "Shared"
            isStatic = true
        }
    }

    android {
        namespace = "com.dc.melodiasmario.shared"
        compileSdk = libs.versions.android.compileSdk.get()
            .toInt()
        minSdk = libs.versions.android.minSdk.get()
            .toInt()

        compilerOptions {
            jvmTarget = JvmTarget.JVM_11
        }
        androidResources {
            enable = true
        }
        withHostTest {
            isIncludeAndroidResources = true
        }
        withDeviceTestBuilder {
            sourceSetTreeName = "test"
        }.configure {
            instrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"
        }
    }

    sourceSets {
        androidMain.dependencies {
            implementation(dependencyNotation = libs.compose.uiToolingPreview)
            implementation(dependencyNotation = libs.compose.uiTooling)

            implementation(dependencyNotation = libs.ktor.client.okhttp)
        }
        commonMain.dependencies {
            implementation(dependencyNotation = project(":core:common"))
            implementation(dependencyNotation = project(":core:presentation"))
            implementation(dependencyNotation = project(":core:common-ui"))
            implementation(dependencyNotation = project(":core:di"))

            implementation(dependencyNotation = project(":feature:playlisttrack:presentation"))
            implementation(dependencyNotation = project(":feature:playlisttrack:ui"))
            implementation(dependencyNotation = project(":feature:currenttrack:presentation"))
            implementation(dependencyNotation = project(":feature:currenttrack:ui"))
            implementation(dependencyNotation = project(":feature:guild:presentation"))
            implementation(dependencyNotation = project(":feature:guild:ui"))
            implementation(dependencyNotation = project(":feature:login:presentation"))
            implementation(dependencyNotation = project(":feature:login:ui"))
            implementation(dependencyNotation = project(":feature:playlist:presentation"))
            implementation(dependencyNotation = project(":feature:playlist:ui"))
            implementation(dependencyNotation = project(":feature:profile:presentation"))
            implementation(dependencyNotation = project(":feature:profile:ui"))
            implementation(dependencyNotation = project(":feature:queue:presentation"))
            implementation(dependencyNotation = project(":feature:queue:ui"))
            implementation(dependencyNotation = project(":feature:removesong:presentation"))
            implementation(dependencyNotation = project(":feature:removesong:ui"))
            implementation(dependencyNotation = project(":feature:settings:presentation"))
            implementation(dependencyNotation = project(":feature:settings:ui"))

            implementation(dependencyNotation = libs.compose.runtime)
            implementation(dependencyNotation = libs.compose.foundation)
            implementation(dependencyNotation = libs.compose.material3)
            implementation(dependencyNotation = libs.compose.ui)
            implementation(dependencyNotation = libs.compose.components.resources)
            implementation(dependencyNotation = libs.compose.uiToolingPreview)
            implementation(dependencyNotation = libs.androidx.lifecycle.viewmodelCompose)
            implementation(dependencyNotation = libs.androidx.lifecycle.runtimeCompose)

            implementation(dependencyNotation = libs.koin.core)
            implementation(dependencyNotation = libs.koin.annotations)
            implementation(dependencyNotation = libs.koin.compose)
            implementation(dependencyNotation = libs.koin.compose.viewmodel)

            implementation(dependencyNotation = libs.ktor.client.core)
            implementation(dependencyNotation = libs.ktor.client.logging)
            implementation(dependencyNotation = libs.ktor.client.content.negotiation)
            implementation(dependencyNotation = libs.ktor.serialization.kotlinx.json)

            implementation(dependencyNotation = libs.jetbrains.navigation3.ui)
            implementation(dependencyNotation = libs.kotlinx.serialization.json)
        }

        nativeMain.dependencies {
            implementation(dependencyNotation = libs.ktor.client.darwin)
        }

        commonTest.dependencies {
            implementation(dependencyNotation = libs.kotlin.test)
        }
    }

    sourceSets.named("commonMain")
        .configure {
            kotlin.srcDir("build/generated/ksp/metadata/commonMain/kotlin")
        }

}

dependencies {
    androidRuntimeClasspath(libs.compose.uiTooling)
}
