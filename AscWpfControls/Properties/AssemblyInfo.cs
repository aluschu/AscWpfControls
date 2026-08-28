using System.Windows;

// Sagt WPF, dass die Default-Templates dieser Bibliothek in Themes/Generic.xaml der eigenen
// Assembly liegen. Ohne dieses Attribut findet ein Control seinen Style zur Laufzeit nicht und
// bleibt unsichtbar - ohne jede Fehlermeldung.
[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]
