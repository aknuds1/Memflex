import subprocess
import os
from glob import glob

for dname in ("FlexProviders", "FlexProviders.Mongo"):
    os.chdir(dname)
    for nupkg in glob("*.nupkg"):
        os.remove(nupkg)
    subprocess.check_call(["nuget", "pack"])
    os.chdir("..")
