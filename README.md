
<div align="center">
<img src="https://github.com/asv-soft/asv-drones-gui-afis/assets/151620493/932425b6-547e-4d35-bf90-6430265c8e97" width="300px" margin-left="200px">  
</div>



[//]: [![Release](https://github.com/asv-soft/asv-drones/actions/workflows/ReleaseDeployAction.yml/badge.svg)](https://github.com/asv-soft/asv-drones/actions/workflows/ReleaseDeployAction.yml)

## 1. Introduction

Asv.Drones: Empowering Innovation in Unmanned Aerial Systems

Welcome to Asv.Drones, an advanced and modular open-source application designed to revolutionize the field of Unmanned Aerial Systems (UAS). Committed to fostering innovation and collaboration, Asv.Drones is not just a drone application; it's a community-driven platform that opens the doors to limitless possibilities.

Key Features:

1. **Modularity:**
   Asv.Drones embraces a modular architecture, allowing users to tailor the application to their specific needs. Each module serves a distinct purpose, contributing to the overall functionality and versatility of the platform.

2. **Open Source Philosophy:**
   Transparency and collaboration lie at the heart of Asv.Drones. The entire application, along with its constituent modules, is open source. This means that not only can users benefit from the software, but they can also actively contribute to its enhancement and evolution.

3. **Module Overview:**

  - **Asv.Drones.Gbs (Ground Base Station Service):**
    This module provides a robust ground base station service, ensuring seamless communication between the drone and the operator on the ground. Open source nature encourages customization for specific ground station requirements. Source code for this module can be found [here](https://github.com/asv-soft/asv-drones-gbs)

  - **Asv.Drones.Sdr (SDR Payload Example Project):**
    Explore the possibilities of Software-Defined Radio (SDR) payloads with this open-source example project. Asv.Drones.Sdr serves as a foundation for integrating cutting-edge SDR technologies into unmanned aerial systems. Source code for this module can be found [here](https://github.com/asv-soft/asv-drones-sdr)

  - **Asv.Gnss (GNSS Library):**
    The Asv.Gnss module is a comprehensive GNSS library that parses RTCMv2, RTCMv3, and NMEA protocols. It goes a step further by providing control over receivers through SBF, ComNav, and UBX protocols, all tailored for .NET environments. Source code for this module can be found [here](https://github.com/asv-soft/asv-gnss)

  - **Asv.Mavlink (Mavlink Library for .NET 6.0, .NET 7.0):**
    For seamless communication and control, Asv.Mavlink is a dedicated library compatible with .NET 6.0 and .NET 7.0. It ensures that your drone's communication adheres to the Mavlink protocol standards. Source code for this module can be found [here](https://github.com/asv-soft/asv-mavlink)

  - **Asv.Common:**
    Asv.Common serves as the backbone, offering common types and extensions for all Asv-based libraries. It streamlines development, ensuring consistency and efficiency across different modules. Source code for this module can be found [here](https://github.com/asv-soft/asv-common)

    
   Here's a schematic representation of the whole project:
<p align="center">
    <img src="img/screenshot-asv-drones-structure.png" alt="structure" width="650" />
</p>

4. **Community Collaboration:**
   Asv.Drones thrives on community collaboration. Developers, enthusiasts, and innovators are encouraged to contribute, share insights, and collectively shape the future of unmanned aerial systems.

Embark on a journey of exploration, experimentation, and innovation with Asv.Drones. Whether you're a developer, researcher, or drone enthusiast, this open-source platform invites you to redefine the possibilities of unmanned aerial systems.


<p align="center">
    <img src="img/screenshot-windows-flights.png" alt="win" width="650" />
    <img src="img/screenshot-ubuntu-flights.png" alt="linux" width="650" />
    <img src="img/screenshot-mac-os-flights.png" alt="mac" width="650" />
    <img src="img/screenshot-android-flights.png" alt="android" width="650" />
    <img src="img/screenshot-planing.png" alt="planning" width="650" />
    <img src="img/screenshot-packet-viewer.png" alt="packet-viewer" width="650" />
    <img src="img/screenshot-connections.png" alt="connections" width="650" />
    <img src="img/screenshot-settings.png" alt="settings" width="650" />
</p>

## 2. Different sets of components
Asv.Drones can work with different combinations of it's components

### Example Of Usage With GBS

**Ground Base Station Integration:** Asv.Drones offers seamless integration with ground base stations through our proprietary implementation called Asv.Drones.Gbs, available on GitHub [here](https://github.com/asv-soft/asv-drones-gbs). Built to operate via the MAVLink protocol, Asv.Drones.Gbs allows users to remotely manage and monitor drone operations from a centralized platform. Moreover, any other ground base station software compatible with MAVLink can seamlessly interface with our application, ensuring flexibility and interoperability across different systems (development of additional UI controls may be required). With Asv.Drones.Gbs, users can plan missions, monitor telemetry data, and adjust flight parameters with ease.

<div align="center">
   <img src="img/asv-drones-gbs-connections.png" alt="gbs-connections" width="650" />
   <img src="img/asv-drones-gbs-settings.png" alt="gbs-parameters" width="650" />
   <img src="img/asv-drones-gbs-widget.png" alt="gbs-widget" width="650" />
   <img src="img/asv-drones-gbs-packets.png" alt="gbs-packets" width="650" />
</div>

### Example Of Usage With SDR

**SDR Integration:** Enhance your drone operations with Asv.Drones.Sdr, our custom-built SDR software available on GitHub [here](https://github.com/asv-soft/asv-drones-sdr). Designed to communicate via the MAVLink protocol, Asv.Drones.Sdr expands the capabilities of your drones beyond traditional control. Additionally, our software allows integration with any other SDR software that utilizes MAVLink, enabling a wide range of applications such as spectrum monitoring, signal intelligence, and radio relay (development of additional UI controls may be required). With Asv.Drones.Sdr, users can leverage SDR technology to scan and analyze radio frequency signals, intercept communication signals, and extend communication networks, empowering them to tackle diverse missions effectively.

<div align="center">
   <img src="img/asv-drones-sdr-connections.png" alt="sdr-connections" width="650" />
   <img src="img/asv-drones-sdr-settings.png" alt="sdr-parameters" width="650" />
   <img src="img/asv-drones-sdr-widget.png" alt="sdr-widget" width="650" />
   <img src="img/asv-drones-sdr-packets.png" alt="sdr-packets" width="650" />
</div>

## 3. Getting Started

### Setting Up the Development Environment

To ensure a smooth development experience, follow the steps below to set up your development environment:

### 3.1 Prerequisites:
- **Operating System:** This project is compatible with Windows, macOS, and Linux. Ensure that your development machine runs one of these supported operating systems.
- **IDE (Integrated Development Environment):** We recommend using [Visual Studio](https://visualstudio.microsoft.com/) or [JetBrains Rider](https://www.jetbrains.com/rider/) as your IDE for C# development. Make sure to install the necessary extensions and plugins for a better development experience.

### 3.2 .NET Installation:
- This project is built using [.NET 6.0](https://dotnet.microsoft.com/download/dotnet/6.0) and [.NET 7.0](https://dotnet.microsoft.com/download/dotnet/7.0), the latests version of the .NET platform. We recommend installing .NET 7.0 by following the instructions provided on the official [.NET website](https://dotnet.microsoft.com/download/dotnet/7.0).

   ```bash
   # Check your current .NET version
   dotnet --version
   ```

### 3.3 Version Control:
- If you haven't already, install a version control system such as [Git](https://git-scm.com/) to track changes and collaborate with other developers.

### 3.4 Clone the Repository:
- Clone the project repository to your local machine using the following command:

   ```bash
   git clone https://github.com/asv-soft/asv-drones.git
   ```

### 3.5 Restore Dependencies:
- Navigate to the platform project directory and restore the required dependencies. There is 3 possible platform directories to build and debug our app: __Asv.Drones.Gui.Desktop__, __Asv.Drones.Gui.Android__, __Asv.Drones.Gui.iOS__.
For example we will use __Asv.Drones.Gui.Desktop__ platform, so you have to execute the following command:

   ```bash
   cd asv-drones/src/Asv.Drones.Gui.Desktop
   dotnet workload restore
   dotnet workload repair
   ```

### 3.6 Build and Run:
- After restore you have to build the project to ensure that everything is set up correctly, and if it's not - try to restore workloads again:

   ```bash
   dotnet build
   ```

- Run the project:

   ```bash
   dotnet run
   ```

Congratulations! Your development environment is now set up, and you are ready to start contributing to the project. If you encounter any issues during the setup process, refer to the project's documentation or reach out to the development team for assistance.

### Building for Android

To build applications for Android, additional setup is required for JDK and Android SDK installation. Follow the instructions below based on your operating system.

#### Windows

1. Install the .NET MAUI Check tool to verify your environment is ready for .NET MAUI development:
   ```
   dotnet tool install -g Redth.Net.Maui.Check
   maui-check
   ```
2. For Android SDK managing we recommend to install Android Studio.
3. Using Android Studio's SDK Manager, download Android 13.0 (Tiramisu) and API level 33. It's highly recommended to create an Android Virtual Device (AVD) with these settings, preferably with tablet configurations for better testing experience.
4. Build the project for Android:
   ```
   dotnet build -t:Run -f net7.0-android /p:AndroidSdkDirectory=${AndroidSdkPath}
   ```
- The `${AndroidSdkPath}` should be replaced with the actual path to your Android SDK installation.

#### Linux

1. Install Android Studio to manage Android SDKs:
   ```
   sudo snap install android-studio --classic
   ```
2. Install OpenJDK 11:
   ```
   sudo apt install openjdk-11-jdk
   ```
3. Using Android Studio's SDK Manager, download Android 13.0 (Tiramisu) and API level 33. It's highly recommended to create an Android Virtual Device (AVD) with these settings, preferably with tablet configurations for better testing experience.
4. Build the project for Android:
   ```
   dotnet build -f net7.0-android /p:AndroidSdkDirectory=${AndroidSdkPath}
   ```
- The `${AndroidSdkPath}` should be replaced with the actual path to your Android SDK installation.

#### MacOS

1. Install Android Studio:
   ```
   brew install --cask android-studio
   ```
2. Install JDK through Homebrew or any preferred method:
   ```
   brew install openjdk
   ```
3. Using Android Studio's SDK Manager, download Android 13.0 (Tiramisu) and API level 33. It's highly recommended to create an Android Virtual Device (AVD) with these settings, preferably with tablet configurations for better testing experience.
4. Build the project for Android, specifying the Android SDK directory:
   ```
   dotnet build -f net7.0-android /p:AndroidSdkDirectory=${AndroidSdkPath}
   ```
- The `${AndroidSdkPath}` should be replaced with the actual path to your Android SDK installation.

### Additional Notes

- If you want to run application after build you should start your previously created AVD and wait until it's startup processes are complete. Then you have to execute following command:
    ```
    dotnet run -f net7.0-android /p:AndroidSdkDirectory=${AndroidSdkPath}
    ```
- The `${AndroidSdkPath}` should be replaced with the actual path to your Android SDK installation.

## 4. Code Structure

The organization of the codebase plays a crucial role in maintaining a clean, scalable, and easily understandable project. This section outlines the structure of our codebase, highlighting key directories and their purposes.

### 4.1 Solution Organization

Our solution is organized the following way:

- **`sln/`:** This directory contains the source code of the application. The code is further organized into projects, each residing in its own subdirectory. The goal is to promote modularity and maintainability.

  ```
  sln/
  ├── Platforms/
  │   ├── Asv.Drones.Gui.Android/
  │   ├── Asv.Drones.Gui.Browser/
  │   ├── Asv.Drones.Gui.Desktop/
  │   └── Asv.Drones.Gui.iOS/
  ├── Asv.Drones.Gui/
  │   ├── Assets/
  │   ├── Views/
  │   └── ...
  ├── Asv.Drones.Gui.Core/
  │   ├── Assets/
  │   ├── Controls/
  │   ├── Services/
  │   ├── Shell/
  │   ├── Tools/
  │   └── ...
  ├── Asv.Drones.Gui.Gbs/
  │   ├── Shell/
  │   └── ...
  ├── Asv.Drones.Gui.Map/
  │   ├── Core/
  │   ├── ViewModels/
  │   └── ...
  ├── Asv.Drones.Gui.Sdr/
  │   ├── Control/
  │   ├── Service/
  │   ├── Shell/
  │   ├── Tools/
  │   └── ...
  └── Asv.Drones.Gui.Uav/
      ├── Actions/
      ├── Controls/
      ├── Shell/
      └── ...

### 4.2 Naming Conventions

Consistent naming conventions are essential for code readability. Throughout the codebase, we follow the guidelines outlined [in our documentation](https://docs.asv.me/use-cases/for-developers)

These conventions contribute to a unified and coherent codebase.

By adhering to this organized structure and naming conventions, we aim to create a codebase that is easy to navigate, scalable, and conducive to collaboration among developers.

## 5. Coding Style

Maintaining a consistent coding style across the project enhances readability, reduces errors, and facilitates collaboration. The following guidelines outline our preferred coding style for C#:

### 5.1 C# Coding Style

#### 5.1.1 Formatting

- **Indentation:** Use tabs for indentation. Each level of indentation should consist of one tab.
- **Brace Placement:** Place opening braces on the same line as the statement they belong to, and closing braces on a new line.

    ```
    // Good
    if (condition)
    {
        // Code here
    }

    // Bad
    if (condition) {
        // Code here
    }
    ```

#### 5.1.2 Naming Conventions

- **Pascal Case:** Use Pascal case for class names, method names, and property names.

    ```
    public class MyClass
    {
        public void MyMethod()
        {
            // Code here
        }

        public int MyProperty { get; set; }
    }
    ```

#### 5.1.3 Language Features

- **Expression-bodied Members:** Utilize expression-bodied members for concise one-liners.

    ```
    // Good
    public int CalculateSquare(int x) => x * x;

    // Bad
    public int CalculateSquare(int x)
    {
        return x * x;
    }
    ```

- **Null Conditional Operator:** Use the null conditional operator (`?.`) for safe property or method access.

    ```
    // Good
    int? length = text?.Length;

    // Bad
    int length = (text != null) ? text.Length : 0;
    ```

### 5.2 Documentation

#### 5.2.1 Comments

- **XML Documentation:** Include XML comments for classes, methods, and properties to provide comprehensive documentation.

    ```
    /// <summary>
    /// Represents a sample class.
    /// </summary>
    public class SampleClass
    {
        /// <summary>
        /// Calculates the sum of two numbers.
        /// </summary>
        /// <param name="a">The first number.</param>
        /// <param name="b">The second number.</param>
        /// <returns>The sum of the two numbers.</returns>
        public int Add(int a, int b)
        {
            // Code here
        }
    }
    ```

#### 5.2.2 Code Comments

- Use comments sparingly and focus on explaining complex or non-intuitive code sections.

By adhering to these coding style guidelines, we aim to create code that is easy to read, understand, and maintain.

## 6. Version Control

Version control is a fundamental aspect of our development process, providing a systematic way to track changes, collaborate with team members, and manage the evolution of our codebase. We utilize Git as our version control system.

### 6.1 Branching Strategy

#### 6.1.1 Feature Branches

For each new feature or bug fix, create a dedicated feature branch. The branch name should be descriptive of the feature or issue it addresses.

```bash
# Example: Creating a new feature branch
git checkout -b feature/my-new-feature
```

#### 6.1.2 Hotfix Branches

In case of critical issues in the production environment, create a hotfix branch. This allows for a quick resolution without affecting the main development branch.

```bash
# Example: Creating a hotfix branch
git checkout -b hotfix/1.0.1
```

### 6.2 Commit Messages

Write clear and concise commit messages that convey the purpose of the change. Follow these guidelines:

- Start with a verb in the imperative mood (e.g., "Add," "Fix," "Update").
- Keep messages short but descriptive.

Example:

```bash
# Good
git commit -m "Add user authentication feature"

# Bad
git commit -m "Updated stuff"
```

### 6.3 Pull Requests

Before merging changes into the main branch, create a pull request (PR). This allows for code review and ensures that changes adhere to coding standards.

- Assign reviewers to the PR.
- Include a clear description of the changes.
- Ensure that automated tests pass before merging.

### 6.4 Merging Strategy

Adopt a merging strategy based on the nature of the changes:

- **Feature Branches:** Merge feature branches into the main branch after code review and approval.
- **Release Branches:** Merge release branches into the main branch and tag the commit for the release.

```bash
# Example: Merging a feature branch
git checkout main
git merge --no-ff feature/my-new-feature
```

### 6.5 Repository Hosting

Our Git repository is hosted on [GitHub](https://github.com/asv-soft/asv-drones-sdr). Ensure that you have the necessary permissions and follow best practices for repository management.

By following these version control practices, we aim to maintain a well-organized and collaborative development process.

## 7. Build and Deployment

The build and deployment processes are crucial components of our development workflow. This section outlines the steps for building the project and deploying it using GitHub Releases.

### 7.1 Build Process

To compile the project, use the following command:

```bash
dotnet build
```

This command compiles the code and produces executable binaries.

### 7.2 Deployment using GitHub Releases

Our application is deployed using [GitHub Releases](https://docs.github.com/en/repositories/releasing-projects-on-github/about-releases).

Latest release can be found [here](https://github.com/asv-soft/asv-drones-sdr/releases).

## 8. Contributing

We welcome contributions from the community to help enhance and improve our project. Before contributing, please take a moment to review this guide.

### 8.1 Code Reviews

All code changes undergo a review process to ensure quality and consistency. Here are the steps to follow:

1. **Fork the Repository:** Start by forking the repository to your own GitHub account.

2. **Create a Feature Branch:** Create a new branch for your feature or bug fix.

   ```bash
   git checkout -b feature/my-feature
   ```

3. **Commit Changes:** Make your changes, commit them with clear and concise messages, and push the branch to your forked repository.

   ```bash
   git commit -m "Add new feature"
   git push origin feature/my-feature
   ```

4. **Open a Pull Request (PR):** Submit a pull request to the main repository, detailing the changes made and any relevant information. Ensure your PR adheres to the established coding standards.

5. **Code Review:** Participate in the code review process by responding to feedback and making necessary adjustments. Addressing comments promptly helps streamline the review process.

6. **Merge:** Once the code review is complete and the changes are approved, your pull request will be merged into the main branch.

### 8.2 Submitting Changes

Before submitting changes, ensure the following:

- **Coding Standards:** Adhere to the coding standards and guidelines outlined in this document.

- **Tests:** If applicable, include tests for your changes and ensure that existing tests pass.

- **Documentation:** Update relevant documentation, including code comments and external documentation, to reflect your changes.

### 8.3 Communication

For larger changes or feature additions, it's beneficial to discuss the proposed changes beforehand. Engage with the community through:

- **Opening an Issue:** Discuss your proposed changes by opening an issue. This provides an opportunity for community input before investing significant time in development.

- **Joining Discussions:** Participate in existing discussions related to the project. Your insights and feedback are valuable.

### 8.4 Contributor License Agreement (CLA)

By contributing to this project, you agree that your contributions will be licensed under the project's license. If a Contributor License Agreement (CLA) is required, it will be provided in the repository.

We appreciate your contributions, and together we can make this project even better!

## 9. Code Documentation

Clear and comprehensive code documentation is essential for ensuring that developers can easily understand, use, and contribute to the project. Follow these guidelines for documenting your code:

### 9.1 Inline Comments

Use inline comments to explain specific sections of your code, especially for complex logic or non-intuitive implementations. Follow these principles:

- **Clarity:** Write comments that enhance code comprehension. If a piece of code is not self-explanatory, provide comments explaining the reasoning or intention.

- **Conciseness:** Keep comments concise and to the point. Avoid unnecessary comments that do not add value.

- **Update Comments:** Regularly review and update comments to reflect any changes in the code. Outdated comments can be misleading.

Example:

```bash
// Calculate the sum of two numbers
int CalculateSum(int a, int b)
{
    return a + b;
}
```

### 9.2 XML Documentation

For classes, methods, properties, and other significant code elements, use XML documentation comments to provide comprehensive information. Follow these guidelines:

- **Summary:** Provide a summary that succinctly describes the purpose of the class or member.

- **Parameters:** Document each parameter, specifying its purpose and any constraints.

- **Returns:** If applicable, document the return value and its significance.

- **Examples:** Include examples that demonstrate how to use the class or member.

Example:

```bash
/// <summary>
/// Represents a utility class for mathematical operations.
/// </summary>
public class MathUtility
{
    /// <summary>
    /// Calculates the sum of two numbers.
    /// </summary>
    /// <param name="a">The first number.</param>
    /// <param name="b">The second number.</param>
    /// <returns>The sum of the two numbers.</returns>
    public int CalculateSum(int a, int b)
    {
        return a + b;
    }
}
```

### 9.3 Consistency

Ensure consistency in your documentation style across the codebase. Consistent documentation makes it easier for developers to navigate and understand the project.

By following these documentation guidelines, we aim to create a codebase that is not only functional but also accessible and easily maintainable for all contributors.

## 10. Security

Ensuring the security of our software is paramount to maintaining the integrity and confidentiality of user data. Developers should adhere to best practices and follow guidelines outlined in this section.

### 10.1 Code Security Practices

#### 10.1.1 Input Validation

Always validate and sanitize user input to prevent injection attacks and ensure the integrity of your application.

```bash
// Example for C#
public ActionResult ProcessUserInput(string userInput)
{
    if (string.IsNullOrWhiteSpace(userInput))
    {
        // Handle invalid input
    }

    // Process input
}
```

#### 10.1.2 Authentication and Authorization

Implement secure authentication and authorization mechanisms to control access to sensitive functionalities and data. Leverage industry-standard protocols like OAuth when applicable.

#### 10.1.3 Secure Communication

Ensure that communication between components, APIs, and external services is encrypted using secure protocols (e.g., HTTPS).

### 10.2 Dependency Security

#### 10.2.1 Dependency Scanning

Regularly scan and update dependencies to identify and address security vulnerabilities. Leverage tools and services that provide automated dependency analysis.

#### 10.2.2 Minimal Dependencies

Keep dependencies to a minimum and only include libraries and packages that are actively maintained and have a good security track record.

### 10.3 Data Protection

#### 10.3.1 Encryption

Sensitive data, both at rest and in transit, should be encrypted. Utilize strong encryption algorithms and ensure proper key management.

#### 10.3.2 Data Backups

Implement regular data backup procedures to prevent data loss in the event of security incidents or system failures.

### 10.4 Secure Coding Standards

Adhere to secure coding standards to mitigate common vulnerabilities. Follow principles such as the [OWASP Top Ten](https://owasp.org/www-project-top-ten/) to address security concerns in your codebase.

### 10.5 Reporting Security Issues

If you discover a security vulnerability or have concerns about the security of the project, please report it immediately to our team at [our telegram channel](https://t.me/asvsoft). Do not disclose security-related issues publicly until they have been addressed.

### 9.6 Security Training

Encourage ongoing security training for all team members to stay informed about the latest security threats and best practices. Knowledgeable developers are key to maintaining a secure codebase.

By incorporating security practices into our development process, we aim to create a robust and secure software environment for our users.

## 11. License

This project is licensed under the terms of the MIT License. A copy of the MIT License is provided in the [LICENSE](https://github.com/asv-soft/asv-drones-sdr?tab=MIT-1-ov-file) file.

### MIT License

```
MIT License

Copyright (c) 2023 Asv Soft

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

### Using the MIT License

The MIT License is a permissive open-source license that allows for the free use, modification, and distribution of the software. It is important to review and understand the terms of the license before using, contributing to, or distributing this software.

By contributing to this project, you agree that your contributions will be licensed under the MIT License.

For more details about the MIT License, please visit [opensource.org/licenses/MIT](https://opensource.org/licenses/MIT).

## 12. Contact

If you have questions, suggestions, or need assistance with the project, we encourage you to reach out through the following channels:

### 12.1 Telegram Channel

Visit our Telegram channel: [ASVSoft on Telegram](https://t.me/asvsoft)

Feel free to join our Telegram community to engage in discussions, seek help, or share your insights.

### 12.2 GitHub Issues

For bug reports, feature requests, or any project-related discussions, please use our GitHub Issues:

[Project Issues on GitHub](https://github.com/asv-soft/asv-drones-sdr/issues)

Our GitHub repository is the central hub for project-related discussions and issue tracking. Please check existing issues before creating new ones to avoid duplication.

### 12.3 Security Concerns

If you discover a security vulnerability or have concerns about the security of the project, please report it immediately to our telegram channel: [ASVSoft on Telegram](https://t.me/asvsoft). Do not disclose security-related issues publicly until they have been addressed.

### 12.4 General Inquiries

For general inquiries or if you prefer email communication, you can reach us at [me@asv.me](mailto:me@asv.me).

We value your feedback and contributions, and we look forward to hearing from you!

## 13. Installing the App for macOS

### 13.1 How to install

1. Go to our [release page](https://github.com/asv-soft/asv-drones/releases) on GitHub.
2. Find ```.dmg``` package in the Assets section.
3. Download it and double click it using your primary mouse button or touchpad.
4. Drag and drop **Asv Drones Gui** into the Application folder.
5. You've successfully installed our application! Congratulations!

*Note: Our application is unsigned, so your system may prevent you from running it.*

### 13.2 How to run the Application

<span style="color: yellow;">
<i>
To run our application you'll need to disable the Gatekeeper. 
This security feature protects your device from unsigned applications (like ours).
While we assure you that our software is virus-free, you should proceed at your own risk.
If you are concerned about potential data loss, you may want to stop here and try installing our app on another system (like Windows).
</i>
</span>

1. Open the terminal and write enter the following command:
````bash
sudo spctl --master-disable
````
*The system may prompt you to enter a password. Use the password for your root user*
2. Go to **System Settings** and find **Security & Privacy** options.
   There, look for a setting that says something like "Allow applications downloaded from:". Select "Anywhere". If this option isn’t available, repeat the first step or try another method to disable Gatekeeper.
3. If the previous step doesn't work, try another command in the terminal:
````bash
sudo xattr -rd com.apple.quarantine /Applications/Asv\ Drones\ Gui.app
````
This command removes the app from the quarantine
4. The application should now work. If this guide didn’t help, please contact us. We’d be happy to assist you!

