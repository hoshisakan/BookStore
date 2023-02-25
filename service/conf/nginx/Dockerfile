FROM nginx:1.23.3

RUN cd / && ln -sf /usr/share/zoneinfo/Asia/Taipei /etc/localtime
RUN rm /etc/nginx/conf.d/default.conf

# Install base packages
RUN apt update && apt upgrade -y
RUN apt install -y wget gnupg2 apt-transport-https \
    ca-certificates curl software-properties-common \
    libnss3-tools iputils-ping telnet net-tools

# Clear package lists
RUN rm -rf /var/lib/apt/lists/*