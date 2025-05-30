{ pkgs ? import <nixpkgs> {} }:

let
  xlibs = with pkgs.xorg; [
    libX11
    libICE
    libSM
    libXext
    libXcursor
    libXi
    libXrandr
    libXxf86vm
  ];
in
pkgs.mkShell {
  buildInputs = with pkgs; [
    dotnet-sdk_9
    fontconfig
    libglvnd
    alsa-lib
  ] ++ xlibs;

  LD_LIBRARY_PATH = pkgs.lib.makeLibraryPath ([
    pkgs.fontconfig
    pkgs.libglvnd
    pkgs.alsa-lib
  ] ++ xlibs);
}
