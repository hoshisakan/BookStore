FROM ubuntu:22.10


RUN apt update && apt upgrade -y && apt install -y locales \
    && localedef -i en_US -c -f UTF-8 -A /usr/share/locale/locale.alias en_US.UTF-8
ENV LANG en_US.utf8

# Install base packages
RUN apt install -y wget gnupg2 apt-transport-https \
    ca-certificates curl software-properties-common \
    libnss3-tools iputils-ping telnet net-tools \
    openssl

# Add the Microsoft package signing key to your list of trusted keys and add the package repository.
RUN wget https://packages.microsoft.com/config/ubuntu/22.10/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN rm packages-microsoft-prod.deb

# Install .NET Core SDK
RUN apt install dotnet-sdk-6.0 -y
# if you want to only use the runtime, uncomment the following line
# RUN apt install aspnetcore-runtime-6.0 -y

# Clear package lists
RUN rm -rf /var/lib/apt/lists/*

# WORKDIR /app
WORKDIR /deploy/HoshiBookWeb

# Run the app
# ENTRYPOINT [ "dotnet", "run", "--project", "HoshiBookWeb" ]
ENTRYPOINT [ "dotnet", "HoshiBookWeb.dll" ]