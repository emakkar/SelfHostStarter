#!/bin/bash
if test "$OS" = "Windows_NT"
then
  # use .Net

  packages/FAKE.4.19.0/tools/FAKE.exe $@ --fsiargs build.fsx
else
  # use mono

  .paket/paket.bootstrapper.exe

	.paket/paket.exe restore

mono packages/FAKE/tools/FAKE.exe $@ --fsiargs -d:MONO build.fsx
fi