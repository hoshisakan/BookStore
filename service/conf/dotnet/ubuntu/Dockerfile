FROM ubuntu:22.10

# Add arguments to set timezone and dotnet version
ARG DOTNET_VERSION
ARG DOTNET_TIME_ZONE
ARG DOTNET_LANG_NAME
ARG DOTNET_LANG_INPUTFILE
ARG DOTNET_LANG_CHARMAP
ARG DEBIAN_FRONTEND
ARG DOTNET_POSTGRESQL_CLIENT_HOME
ARG DOTNET_POSTGRESQL_CLIENT_VERSION
ARG POSTGRES_DATA_BACKUP_PATH
ARG DOTNET_PACKAGES_PATH

# Set environment variable for postgresql-client specific version
ENV PATH=$DOTNET_POSTGRESQL_CLIENT_HOME/bin:$PATH

# Install base packages, build-essential libreadline-dev zlib1g-dev for make install postgresql-client specific version
RUN apt update && apt install -y wget locales gnupg2 apt-transport-https \
    ca-certificates curl software-properties-common \
    libnss3-tools iputils-ping telnet net-tools \
    openssl tzdata build-essential libreadline-dev zlib1g-dev

# Add the Microsoft package signing key to your list of trusted keys and add the package repository
RUN wget https://packages.microsoft.com/config/ubuntu/22.10/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN rm packages-microsoft-prod.deb

RUN apt install -y dotnet-sdk-${DOTNET_VERSION}.0

# Install dotnet-ef tool for dotnet core specific version
RUN dotnet tool install --global dotnet-ef --version ${DOTNET_VERSION}.*

# Clean apt cache
RUN rm -rf /var/cache/apt && apt-get clean

# Set locale to specified language
RUN localedef -i ${DOTNET_LANG_INPUTFILE} -c -f ${DOTNET_LANG_CHARMAP} -A /usr/share/locale/locale.alias ${DOTNET_LANG_NAME}

# Set timezone to Asia/Taipei
RUN ln -sf /usr/share/zoneinfo/${DOTNET_TIME_ZONE} /etc/localtime

# Reset tzdata software package let user set timezone take effect
RUN dpkg-reconfigure -f noninteractive tzdata

# Create directory then change directory to it
WORKDIR ${DOTNET_PACKAGES_PATH}

# Download postgresql-client specific version
RUN wget https://ftp.postgresql.org/pub/source/v${DOTNET_POSTGRESQL_CLIENT_VERSION}/postgresql-${DOTNET_POSTGRESQL_CLIENT_VERSION}.tar.gz

# Unzip postgresql-client specific version
RUN tar -zxvf postgresql-${DOTNET_POSTGRESQL_CLIENT_VERSION}.tar.gz -C ${DOTNET_PACKAGES_PATH}

WORKDIR ${DOTNET_PACKAGES_PATH}/postgresql-${DOTNET_POSTGRESQL_CLIENT_VERSION}

# Install postgresql-client specific version
RUN ./configure --prefix=${DOTNET_POSTGRESQL_CLIENT_HOME} && make && make install

# Remove postgresql-client specific version install package files
RUN rm -rf ${DOTNET_PACKAGES_PATH}/postgresql-${DOTNET_POSTGRESQL_CLIENT_VERSION}.tar.gz && rm -rf ${DOTNET_PACKAGES_PATH}/postgresql-${DOTNET_POSTGRESQL_CLIENT_VERSION}

# Clear package lists
RUN rm -rf /var/lib/apt/lists/*

RUN mkdir -p ${POSTGRES_DATA_BACKUP_PATH}

# WORKDIR /app
WORKDIR /deploy/HoshiBookWeb

# Run the app
ENTRYPOINT [ "dotnet", "HoshiBookWeb.dll" ]