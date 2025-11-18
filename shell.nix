{ pkgs ? import <nixpkgs> {} }:

pkgs.mkShell {
  buildInputs = [
    pkgs.cloc
    pkgs.dotnet-sdk_9
    pkgs.skia
  ];

  shellHook = ''
    echo Avalonix shell:\n
  '';
}
