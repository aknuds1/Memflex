import subprocess
import os.path
from glob import glob

os.chdir(os.path.dirname(__file__))
for dname in ("FlexProviders", "FlexProviders.Mongo"):
    os.chdir(dname)
    for nupkg in glob("*.nupkg"):
        os.remove(nupkg)
    subprocess.check_call(["nuget", "pack"])
    os.chdir("..")
