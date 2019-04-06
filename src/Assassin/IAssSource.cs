using JetBrains.Annotations;

namespace Assassin {
    /// <summary>
    /// A source that provides ASS subtitle data.
    /// </summary>
    public interface IAssSource {

        [CanBeNull]
        AssTrack CreateTrack([NotNull] AssLibrary library);

    }
}
