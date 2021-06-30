//     _                _      _  ____   _                           _____
//    / \    _ __  ___ | |__  (_)/ ___| | |_  ___   __ _  _ __ ___  |  ___|__ _  _ __  _ __ ___
//   / _ \  | '__|/ __|| '_ \ | |\___ \ | __|/ _ \ / _` || '_ ` _ \ | |_  / _` || '__|| '_ ` _ \
//  / ___ \ | |  | (__ | | | || | ___) || |_|  __/| (_| || | | | | ||  _|| (_| || |   | | | | | |
// /_/   \_\|_|   \___||_| |_||_||____/  \__|\___| \__,_||_| |_| |_||_|   \__,_||_|   |_| |_| |_|
// |
// Copyright 2015-2021 Łukasz "JustArchi" Domeradzki
// Contact: JustArchi@JustArchi.net
// |
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// |
// http://www.apache.org/licenses/LICENSE-2.0
// |
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Helpers;
using ArchiSteamFarm.Localization;
using ArchiSteamFarm.Steam.Integration;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SteamKit2;

namespace ArchiSteamFarm.Storage {
	[SuppressMessage("ReSharper", "ClassCannotBeInstantiated")]
	public sealed class GlobalConfig {
		[PublicAPI]
		public const bool DefaultAutoRestart = true;

		[PublicAPI]
		public const string? DefaultCommandPrefix = "!";

		[PublicAPI]
		public const byte DefaultConfirmationsLimiterDelay = 10;

		[PublicAPI]
		public const byte DefaultConnectionTimeout = 90;

		[PublicAPI]
		public const string? DefaultCurrentCulture = null;

		[PublicAPI]
		public const bool DefaultDebug = false;

		[PublicAPI]
		public const byte DefaultFarmingDelay = 15;

		[PublicAPI]
		public const byte DefaultGiftsLimiterDelay = 1;

		[PublicAPI]
		public const bool DefaultHeadless = false;

		[PublicAPI]
		public const byte DefaultIdleFarmingPeriod = 8;

		[PublicAPI]
		public const byte DefaultInventoryLimiterDelay = 3;

		[PublicAPI]
		public const bool DefaultIPC = true;

		[PublicAPI]
		public const string? DefaultIPCPassword = null;

		[PublicAPI]
		public const ArchiCryptoHelper.EHashingMethod DefaultIPCPasswordFormat = ArchiCryptoHelper.EHashingMethod.PlainText;

		[PublicAPI]
		public const byte DefaultLoginLimiterDelay = 10;

		[PublicAPI]
		public const byte DefaultMaxFarmingTime = 10;

		[PublicAPI]
		public const byte DefaultMaxTradeHoldDuration = 15;

		[PublicAPI]
		public const EOptimizationMode DefaultOptimizationMode = EOptimizationMode.MaxPerformance;

		[PublicAPI]
		public const bool DefaultStatistics = true;

		[PublicAPI]
		public const string? DefaultSteamMessagePrefix = "/me ";

		[PublicAPI]
		public const ulong DefaultSteamOwnerID = 0;

		[PublicAPI]
		public const ProtocolTypes DefaultSteamProtocols = ProtocolTypes.All;

		[PublicAPI]
		public const EUpdateChannel DefaultUpdateChannel = EUpdateChannel.Stable;

		[PublicAPI]
		public const byte DefaultUpdatePeriod = 24;

		[PublicAPI]
		public const ushort DefaultWebLimiterDelay = 300;

		[PublicAPI]
		public const string? DefaultWebProxyPassword = null;

		[PublicAPI]
		public const string? DefaultWebProxyText = null;

		[PublicAPI]
		public const string? DefaultWebProxyUsername = null;

		[PublicAPI]
		public static readonly ImmutableHashSet<uint> DefaultBlacklist = ImmutableHashSet<uint>.Empty;

		[JsonIgnore]
		[PublicAPI]
		public WebProxy? WebProxy {
			get {
				if (BackingWebProxy != null) {
					return BackingWebProxy;
				}

				if (string.IsNullOrEmpty(WebProxyText)) {
					return null;
				}

				Uri uri;

				try {
					uri = new Uri(WebProxyText!);
				} catch (UriFormatException e) {
					ASF.ArchiLogger.LogGenericException(e);

					return null;
				}

				WebProxy proxy = new() {
					Address = uri,
					BypassProxyOnLocal = true
				};

				if (!string.IsNullOrEmpty(WebProxyUsername) || !string.IsNullOrEmpty(WebProxyPassword)) {
					NetworkCredential credentials = new();

					if (!string.IsNullOrEmpty(WebProxyUsername)) {
						credentials.UserName = WebProxyUsername;
					}

					if (!string.IsNullOrEmpty(WebProxyPassword)) {
						credentials.Password = WebProxyPassword;
					}

					proxy.Credentials = credentials;
				}

				BackingWebProxy = proxy;

				return proxy;
			}
		}

		[JsonProperty(Required = Required.DisallowNull)]
		public bool AutoRestart { get; private set; } = DefaultAutoRestart;

		[JsonProperty(Required = Required.DisallowNull)]
		public ImmutableHashSet<uint> Blacklist { get; private set; } = DefaultBlacklist;

		[JsonProperty]
		public string? CommandPrefix { get; private set; } = DefaultCommandPrefix;

		[JsonProperty(Required = Required.DisallowNull)]
		[Range(byte.MinValue, byte.MaxValue)]
		public byte ConfirmationsLimiterDelay { get; private set; } = DefaultConfirmationsLimiterDelay;

		[JsonProperty(Required = Required.DisallowNull)]
		[Range(1, byte.MaxValue)]
		public byte ConnectionTimeout { get; private set; } = DefaultConnectionTimeout;

		[JsonProperty]
		public string? CurrentCulture { get; private set; } = DefaultCurrentCulture;

		[JsonProperty(Required = Required.DisallowNull)]
		public bool Debug { get; private set; } = DefaultDebug;

		[JsonProperty(Required = Required.DisallowNull)]
		[Range(1, byte.MaxValue)]
		public byte FarmingDelay { get; private set; } = DefaultFarmingDelay;

		[JsonProperty(Required = Required.DisallowNull)]
		[Range(byte.MinValue, byte.MaxValue)]
		public byte GiftsLimiterDelay { get; private set; } = DefaultGiftsLimiterDelay;

		[JsonProperty(Required = Required.DisallowNull)]
		public bool Headless { get; private set; } = DefaultHeadless;

		[JsonProperty(Required = Required.DisallowNull)]
		[Range(byte.MinValue, byte.MaxValue)]
		public byte IdleFarmingPeriod { get; private set; } = DefaultIdleFarmingPeriod;

		[JsonProperty(Required = Required.DisallowNull)]
		[Range(byte.MinValue, byte.MaxValue)]
		public byte InventoryLimiterDelay { get; private set; } = DefaultInventoryLimiterDelay;

		[JsonProperty(Required = Required.DisallowNull)]
		public bool IPC { get; private set; } = DefaultIPC;

		[JsonProperty]
		public string? IPCPassword { get; private set; } = DefaultIPCPassword;

		[JsonProperty(Required = Required.DisallowNull)]
		public ArchiCryptoHelper.EHashingMethod IPCPasswordFormat { get; private set; } = DefaultIPCPasswordFormat;

		[JsonProperty(Required = Required.DisallowNull)]
		[Range(byte.MinValue, byte.MaxValue)]
		public byte LoginLimiterDelay { get; private set; } = DefaultLoginLimiterDelay;

		[JsonProperty(Required = Required.DisallowNull)]
		[Range(1, byte.MaxValue)]
		public byte MaxFarmingTime { get; private set; } = DefaultMaxFarmingTime;

		[JsonProperty(Required = Required.DisallowNull)]
		[Range(byte.MinValue, byte.MaxValue)]
		public byte MaxTradeHoldDuration { get; private set; } = DefaultMaxTradeHoldDuration;

		[JsonProperty(Required = Required.DisallowNull)]
		public EOptimizationMode OptimizationMode { get; private set; } = DefaultOptimizationMode;

		[JsonProperty(Required = Required.DisallowNull)]
		public bool Statistics { get; private set; } = DefaultStatistics;

		[JsonProperty]
		[MaxLength(SteamChatMessage.MaxMessagePrefixBytes / 4)]
		public string? SteamMessagePrefix { get; private set; } = DefaultSteamMessagePrefix;

		[JsonProperty(Required = Required.DisallowNull)]
		public ulong SteamOwnerID { get; private set; } = DefaultSteamOwnerID;

		[JsonProperty(Required = Required.DisallowNull)]
		public ProtocolTypes SteamProtocols { get; private set; } = DefaultSteamProtocols;

		[JsonProperty(Required = Required.DisallowNull)]
		public EUpdateChannel UpdateChannel { get; private set; } = DefaultUpdateChannel;

		[JsonProperty(Required = Required.DisallowNull)]
		[Range(byte.MinValue, byte.MaxValue)]
		public byte UpdatePeriod { get; private set; } = DefaultUpdatePeriod;

		[JsonProperty(Required = Required.DisallowNull)]
		[Range(ushort.MinValue, ushort.MaxValue)]
		public ushort WebLimiterDelay { get; private set; } = DefaultWebLimiterDelay;

		[JsonProperty(PropertyName = nameof(WebProxy))]
		public string? WebProxyText { get; private set; } = DefaultWebProxyText;

		[JsonProperty]
		public string? WebProxyUsername { get; private set; } = DefaultWebProxyUsername;

		[JsonExtensionData]
		internal Dictionary<string, JToken>? AdditionalProperties {
			get;
			[UsedImplicitly]
			set;
		}

		internal bool IsWebProxyPasswordSet { get; private set; }
		internal bool Saving { get; set; }

		[JsonProperty]
		internal string? WebProxyPassword {
			get => BackingWebProxyPassword;

			set {
				IsWebProxyPasswordSet = true;
				BackingWebProxyPassword = value;
			}
		}

		private WebProxy? BackingWebProxy;
		private string? BackingWebProxyPassword = DefaultWebProxyPassword;

		[JsonProperty(PropertyName = SharedInfo.UlongCompatibilityStringPrefix + nameof(SteamOwnerID), Required = Required.DisallowNull)]
		private string SSteamOwnerID {
			get => SteamOwnerID.ToString(CultureInfo.InvariantCulture);

			set {
				if (string.IsNullOrEmpty(value) || !ulong.TryParse(value, out ulong result)) {
					ASF.ArchiLogger.LogGenericError(string.Format(CultureInfo.CurrentCulture, Strings.ErrorIsInvalid, nameof(SSteamOwnerID)));

					return;
				}

				SteamOwnerID = result;
			}
		}

		[JsonConstructor]
		internal GlobalConfig() { }

		internal (bool Valid, string? ErrorMessage) CheckValidation() {
			if (Blacklist.Contains(0)) {
				return (false, string.Format(CultureInfo.CurrentCulture, Strings.ErrorConfigPropertyInvalid, nameof(Blacklist), 0));
			}

			if (ConnectionTimeout == 0) {
				return (false, string.Format(CultureInfo.CurrentCulture, Strings.ErrorConfigPropertyInvalid, nameof(ConnectionTimeout), ConnectionTimeout));
			}

			if (FarmingDelay == 0) {
				return (false, string.Format(CultureInfo.CurrentCulture, Strings.ErrorConfigPropertyInvalid, nameof(FarmingDelay), FarmingDelay));
			}

			if (!Enum.IsDefined(typeof(ArchiCryptoHelper.EHashingMethod), IPCPasswordFormat)) {
				return (false, string.Format(CultureInfo.CurrentCulture, Strings.ErrorConfigPropertyInvalid, nameof(IPCPasswordFormat), IPCPasswordFormat));
			}

			if (MaxFarmingTime == 0) {
				return (false, string.Format(CultureInfo.CurrentCulture, Strings.ErrorConfigPropertyInvalid, nameof(MaxFarmingTime), MaxFarmingTime));
			}

			if (!Enum.IsDefined(typeof(EOptimizationMode), OptimizationMode)) {
				return (false, string.Format(CultureInfo.CurrentCulture, Strings.ErrorConfigPropertyInvalid, nameof(OptimizationMode), OptimizationMode));
			}

			if (!string.IsNullOrEmpty(SteamMessagePrefix) && !SteamChatMessage.IsValidPrefix(SteamMessagePrefix!)) {
				return (false, string.Format(CultureInfo.CurrentCulture, Strings.ErrorConfigPropertyInvalid, nameof(SteamMessagePrefix), SteamMessagePrefix));
			}

			if ((SteamOwnerID != 0) && !new SteamID(SteamOwnerID).IsIndividualAccount) {
				return (false, string.Format(CultureInfo.CurrentCulture, Strings.ErrorConfigPropertyInvalid, nameof(SteamOwnerID), SteamOwnerID));
			}

			if (SteamProtocols is <= 0 or > ProtocolTypes.All) {
				return (false, string.Format(CultureInfo.CurrentCulture, Strings.ErrorConfigPropertyInvalid, nameof(SteamProtocols), SteamProtocols));
			}

			return Enum.IsDefined(typeof(EUpdateChannel), UpdateChannel) ? (true, null) : (false, string.Format(CultureInfo.CurrentCulture, Strings.ErrorConfigPropertyInvalid, nameof(UpdateChannel), UpdateChannel));
		}

		internal static async Task<(GlobalConfig? GlobalConfig, string? LatestJson)> Load(string filePath) {
			if (string.IsNullOrEmpty(filePath)) {
				throw new ArgumentNullException(nameof(filePath));
			}

			if (!File.Exists(filePath)) {
				return (null, null);
			}

			string json;
			GlobalConfig? globalConfig;

			try {
				json = await Compatibility.File.ReadAllTextAsync(filePath).ConfigureAwait(false);

				if (string.IsNullOrEmpty(json)) {
					ASF.ArchiLogger.LogGenericError(string.Format(CultureInfo.CurrentCulture, Strings.ErrorIsEmpty, nameof(json)));

					return (null, null);
				}

				globalConfig = JsonConvert.DeserializeObject<GlobalConfig>(json);
			} catch (Exception e) {
				ASF.ArchiLogger.LogGenericException(e);

				return (null, null);
			}

			if (globalConfig == null) {
				ASF.ArchiLogger.LogNullError(nameof(globalConfig));

				return (null, null);
			}

			(bool valid, string? errorMessage) = globalConfig.CheckValidation();

			if (!valid) {
				if (!string.IsNullOrEmpty(errorMessage)) {
					ASF.ArchiLogger.LogGenericError(errorMessage!);
				}

				return (null, null);
			}

			if (!Program.ConfigMigrate) {
				return (globalConfig, null);
			}

			globalConfig.Saving = true;
			string latestJson = JsonConvert.SerializeObject(globalConfig, Formatting.Indented);
			globalConfig.Saving = false;

			return (globalConfig, json != latestJson ? latestJson : null);
		}

		internal static async Task<bool> Write(string filePath, GlobalConfig globalConfig) {
			if (string.IsNullOrEmpty(filePath)) {
				throw new ArgumentNullException(nameof(filePath));
			}

			if (globalConfig == null) {
				throw new ArgumentNullException(nameof(globalConfig));
			}

			string json = JsonConvert.SerializeObject(globalConfig, Formatting.Indented);

			return await SerializableFile.Write(filePath, json).ConfigureAwait(false);
		}

		public enum EOptimizationMode : byte {
			MaxPerformance,
			MinMemoryUsage
		}

		public enum EUpdateChannel : byte {
			None,
			Stable,

			[PublicAPI]
			Experimental
		}

		// ReSharper disable UnusedMember.Global
		public bool ShouldSerializeAutoRestart() => !Saving || (AutoRestart != DefaultAutoRestart);
		public bool ShouldSerializeBlacklist() => !Saving || ((Blacklist != DefaultBlacklist) && !Blacklist.SetEquals(DefaultBlacklist));
		public bool ShouldSerializeCommandPrefix() => !Saving || (CommandPrefix != DefaultCommandPrefix);
		public bool ShouldSerializeConfirmationsLimiterDelay() => !Saving || (ConfirmationsLimiterDelay != DefaultConfirmationsLimiterDelay);
		public bool ShouldSerializeConnectionTimeout() => !Saving || (ConnectionTimeout != DefaultConnectionTimeout);
		public bool ShouldSerializeCurrentCulture() => !Saving || (CurrentCulture != DefaultCurrentCulture);
		public bool ShouldSerializeDebug() => !Saving || (Debug != DefaultDebug);
		public bool ShouldSerializeFarmingDelay() => !Saving || (FarmingDelay != DefaultFarmingDelay);
		public bool ShouldSerializeGiftsLimiterDelay() => !Saving || (GiftsLimiterDelay != DefaultGiftsLimiterDelay);
		public bool ShouldSerializeHeadless() => !Saving || (Headless != DefaultHeadless);
		public bool ShouldSerializeIdleFarmingPeriod() => !Saving || (IdleFarmingPeriod != DefaultIdleFarmingPeriod);
		public bool ShouldSerializeInventoryLimiterDelay() => !Saving || (InventoryLimiterDelay != DefaultInventoryLimiterDelay);
		public bool ShouldSerializeIPC() => !Saving || (IPC != DefaultIPC);
		public bool ShouldSerializeIPCPassword() => Saving && (IPCPassword != DefaultIPCPassword);
		public bool ShouldSerializeIPCPasswordFormat() => !Saving || (IPCPasswordFormat != DefaultIPCPasswordFormat);
		public bool ShouldSerializeLoginLimiterDelay() => !Saving || (LoginLimiterDelay != DefaultLoginLimiterDelay);
		public bool ShouldSerializeMaxFarmingTime() => !Saving || (MaxFarmingTime != DefaultMaxFarmingTime);
		public bool ShouldSerializeMaxTradeHoldDuration() => !Saving || (MaxTradeHoldDuration != DefaultMaxTradeHoldDuration);
		public bool ShouldSerializeOptimizationMode() => !Saving || (OptimizationMode != DefaultOptimizationMode);
		public bool ShouldSerializeSSteamOwnerID() => !Saving;
		public bool ShouldSerializeStatistics() => !Saving || (Statistics != DefaultStatistics);
		public bool ShouldSerializeSteamMessagePrefix() => !Saving || (SteamMessagePrefix != DefaultSteamMessagePrefix);
		public bool ShouldSerializeSteamOwnerID() => !Saving || (SteamOwnerID != DefaultSteamOwnerID);
		public bool ShouldSerializeSteamProtocols() => !Saving || (SteamProtocols != DefaultSteamProtocols);
		public bool ShouldSerializeUpdateChannel() => !Saving || (UpdateChannel != DefaultUpdateChannel);
		public bool ShouldSerializeUpdatePeriod() => !Saving || (UpdatePeriod != DefaultUpdatePeriod);
		public bool ShouldSerializeWebLimiterDelay() => !Saving || (WebLimiterDelay != DefaultWebLimiterDelay);
		public bool ShouldSerializeWebProxyPassword() => Saving && IsWebProxyPasswordSet && (WebProxyPassword != DefaultWebProxyPassword);
		public bool ShouldSerializeWebProxyText() => !Saving || (WebProxyText != DefaultWebProxyText);
		public bool ShouldSerializeWebProxyUsername() => !Saving || (WebProxyUsername != DefaultWebProxyUsername);

		// ReSharper restore UnusedMember.Global
	}
}
