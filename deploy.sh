#!/bin/bash
pupnet -y --kind appimage -r linux-x64
pupnet -y --kind zip -r win-x64
pupnet -y --kind zip -r osx-x64
pupnet -y --kind zip -r osx-arm64