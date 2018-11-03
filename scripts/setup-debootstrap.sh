#!/usr/bin/env bash
set -e

# apt-get install -y debootstrap qemu-user-static

# create vars
SCRIPT=$(readlink -f "$0")
BASE_DIR=$(dirname "$SCRIPT")
NEW_ROOT=$(dirname "$BASE_DIR")/externals/debootstrap/armhf

# clean up old bits
rm -rf $NEW_ROOT
mkdir -p $NEW_ROOT
cd $NEW_ROOT

# download the new root
qemu-debootstrap --foreign --arch armhf jessie $NEW_ROOT http://ftp.debian.org/debian

# install packages
chroot $NEW_ROOT apt install -y libfontconfig1-dev
