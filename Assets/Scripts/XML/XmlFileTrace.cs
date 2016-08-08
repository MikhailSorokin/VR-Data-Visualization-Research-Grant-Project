using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

/// <summary>
/// For every child that one has, they need to 
/// </summary>
public class XmlFileTrace
{
    private static AuthorInformation authors;
	private static ConferenceInformation conferences;
    private static TitleInformation titles;
    private static YearInformation years;

    public class AuthorInformation : IDisposable
    {
        private IEnumerator<IEnumerable<string>> authorInfo;

        public AuthorInformation(IEnumerator<IEnumerable<string>> article)
        {
            authorInfo = article;
        }

        public void Dispose()
        {
            if (authorInfo != null) authorInfo.Dispose();
        }

        public IEnumerable<string> GetNextXMLAttribute()
        {
            if (authorInfo != null)
            {
                if (authorInfo.MoveNext())
                {
                    return authorInfo.Current;
                }
                Dispose();
            }

            return null;
        }

    }

    public class TitleInformation : IDisposable
    {
        private IEnumerator<string> titleInfo;

        public TitleInformation(IEnumerator<string> titles)
        {
            titleInfo = titles;
        }

        public void Dispose()
        {
            if (titleInfo != null) titleInfo.Dispose();
        }

        public string GetNextXMLAttribute()
        {
            if (titleInfo != null)
            {
                if (titleInfo.MoveNext())
                {
                    return titleInfo.Current;
                }
                Dispose();
            }

            return null;
        }

    }

	public class ConferenceInformation : IDisposable
    {
        private IEnumerator<string> conferenceInfo;

		public ConferenceInformation(IEnumerator<string> conferences)
        {
			conferenceInfo = conferences;
        }

        public void Dispose()
        {
			if (conferenceInfo != null) conferenceInfo.Dispose();
        }

        public string GetNextXMLAttribute()
        {
			if (conferenceInfo != null)
            {
				if (conferenceInfo.MoveNext())
                {
					return conferenceInfo.Current;
                }
                Dispose();
            }

            return null;
        }

    }

    public class YearInformation : IDisposable
    {
        private IEnumerator<string> yearInfo;

        public YearInformation(IEnumerator<string> years)
        {
            yearInfo = years;
        }

        public void Dispose()
        {
            if (yearInfo != null) yearInfo.Dispose();
        }

        public string GetNextXMLAttribute()
        {
            if (yearInfo != null)
            {
                if (yearInfo.MoveNext())
                {
                    return yearInfo.Current;
                }
                Dispose();
            }

            return null;
        }

    }

    /// <summary>
    /// Gets all the input enumerables and creates class enumerators out of them, which are quickly traversable.
    /// </summary>
    /// <param name="_authors">An enumerable of all of every author in the article, where they may be more than one in a particular XMlElement.</param>
    /// <param name="_URLs">An enumerable of all of every url in the article, there is at most one per node.</param>
    /// <param name="_titles">An enumerable of all of every title in the article, there is at most one per node.</param>
    /// <param name="_years">An enumerable of all of every year in the article, there is at most one per node.</param>
    internal static void SetListsFromEnumerables(IEnumerable<IEnumerable<string>> _authors, IEnumerable<string> _conferences, IEnumerable<string> _titles, IEnumerable<string> _years)
    {
        authors = new AuthorInformation(_authors.GetEnumerator());
        conferences = new ConferenceInformation(_conferences.GetEnumerator());
        titles = new TitleInformation(_titles.GetEnumerator());
        years = new YearInformation(_years.GetEnumerator());
    }

    public static AuthorInformation AuthorsEnumerator
    {
        get { return authors; }
    }

	public static ConferenceInformation ConferenceEnumerator
    {
        get { return conferences; }
    }

    public static TitleInformation TitlesEnumerator
    {
        get { return titles; }
    }

    public static YearInformation YearsEnumerator
    {
        get { return years; }
    }

}