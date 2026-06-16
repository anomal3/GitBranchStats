plugins {
    id("org.jetbrains.kotlin.jvm") version "2.1.0"
    id("org.jetbrains.intellij.platform") version "2.16.0"
}

group = "com.gitbranchstats"

// Auto-increment build number on each buildPlugin run
val buildNumberFile = file("build-number.txt")
val currentBuild = if (buildNumberFile.exists()) buildNumberFile.readText().trim().toIntOrNull() ?: 0 else 0
val nextBuild = currentBuild + 1
version = "1.0.$nextBuild"

tasks.named("buildPlugin") {
    doFirst { buildNumberFile.writeText(nextBuild.toString()) }
}

kotlin {
    jvmToolchain(21)
}

repositories {
    mavenCentral()
    intellijPlatform {
        defaultRepositories()
    }
}

dependencies {
    intellijPlatform {
        rider("2025.3.2") { useInstaller = false }
        bundledPlugin("Git4Idea")
        pluginVerifier()
    }
}

intellijPlatform {
    pluginConfiguration {
        id = "com.gitbranchstats.rider"
        name = "Git Branch Stats for Rider"
        // version inherited from project-level `version` (auto-incremented)
        description = """
            <p>Git branch statistics inside JetBrains Rider: author activity, commit timeline charts
            by day, branch switching, and branch comparison — all in one themed tool window.</p>
            <ul>
              <li><b>Author statistics</b> — commits, files changed, lines added/deleted per author</li>
              <li><b>Commit timeline chart</b> — see who committed when, broken down by day</li>
              <li><b>Branch switching</b> — checkout any local branch without leaving the IDE</li>
              <li><b>Branch comparison</b> — commits ahead/behind and full diff statistics</li>
            </ul>
        """.trimIndent()

        ideaVersion {
            sinceBuild = "253"
            untilBuild = "253.*"
        }
    }
}
