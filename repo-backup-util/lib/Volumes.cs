namespace RepoBackupUtil.Lib
{
    public static class Volumes
    {
        public static IEnumerable<DriveInfo> GetVolumes()
        {
            return DriveInfo.GetDrives();
        }
    }
}
